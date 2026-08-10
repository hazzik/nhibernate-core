using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1162
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(
				rc =>
				{
					rc.Table("GH1162Parent");
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
					rc.Bag(
						x => x.Children,
						m =>
						{
							m.Key(k => k.Column("ParentId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Table("GH1162Child");
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parentWithChildren = new Parent { Id = 1, Name = "ParentWithChildren" };
			parentWithChildren.Children.Add(new Child { Id = 1, Name = "Child1" });
			parentWithChildren.Children.Add(new Child { Id = 2, Name = "Child2" });
			session.Save(parentWithChildren);

			var parentWithoutChildren = new Parent { Id = 2, Name = "ParentWithoutChildren" };
			session.Save(parentWithoutChildren);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Child").ExecuteUpdate();
			session.CreateQuery("delete from Parent").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void DistinctRootEntityResultTransformerReturnsRootEntityWithoutRepeatingAddEntity()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var result = session
				.CreateSQLQuery(
					"select {p.*}, {c.*} from GH1162Parent p left outer join GH1162Child c on c.ParentId = p.Id")
				.AddEntity("p", typeof(Parent))
				.AddJoin("c", "p.Children")
				.SetResultTransformer(new DistinctRootEntityResultTransformer())
				.List<Parent>();

			transaction.Commit();

			// Two distinct root entities are expected (the parent with two children counted once,
			// plus the childless parent), each actually being a Parent.
			Assert.That(result, Has.Count.EqualTo(2));
			Assert.That(result.Select(x => x.Id), Is.EquivalentTo(new[] { 1, 2 }));
		}
	}
}
