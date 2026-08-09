using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Mapping.ByCode;
using NHibernate.SqlCommand;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1327
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Animal>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.HighLow));
					rc.Property(x => x.Name);
				});

			mapper.UnionSubclass<Cat>(
				rc =>
				{
					rc.Table("GH1327Cat");
					rc.ManyToOne(x => x.Box, m =>
					{
						m.Column("BoxId");
						m.Class(typeof(CatBox));
						// The wrong table is otherwise picked as the many-to-one
						// target when two union-subclasses reference different
						// concrete types through a property declared on their
						// common, non-union-subclassed base entity. Avoid tripping
						// over that unrelated issue by skipping FK generation.
						m.ForeignKey("none");
					});
				});

			mapper.UnionSubclass<Dog>(
				rc =>
				{
					rc.Table("GH1327Dog");
					rc.ManyToOne(x => x.Box, m =>
					{
						m.Column("BoxId");
						m.Class(typeof(DogBox));
						m.ForeignKey("none");
					});
				});

			mapper.Class<Box>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.HighLow));
					rc.Property(x => x.Name);
				});

			mapper.UnionSubclass<CatBox>(rc => rc.Table("GH1327CatBox"));
			mapper.UnionSubclass<DogBox>(rc => rc.Table("GH1327DogBox"));

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var catBox = new CatBox { Name = "Test" };
			session.Save(catBox);
			session.Save(new Cat { Name = "Tom", Box = catBox });

			var dogBox = new DogBox { Name = "Other" };
			session.Save(dogBox);
			session.Save(new Dog { Name = "Rex", Box = dogBox });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Animal").ExecuteUpdate();
			session.CreateQuery("delete from Box").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanInnerJoinReferredEntityInUnionSubclass()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var animals = session.CreateCriteria<Animal>()
				.CreateAlias("Box", "box", JoinType.LeftOuterJoin)
				.Add(Restrictions.Eq("box.Name", "Test"))
				.List<Animal>();

			Assert.That(animals.Select(a => a.Name), Is.EquivalentTo(new[] { "Tom" }));
		}
	}
}
