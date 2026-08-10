using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1039
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Bag(
						x => x.Children,
						m =>
						{
							m.Key(k => k.Column("EntityId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var entity = new Entity();
			entity.Children.Add(new Child { Name = "Alpha" });
			entity.Children.Add(new Child { Name = "Beta" });
			session.Save(entity);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Child").ExecuteUpdate();
			session.CreateQuery("delete from Entity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanCountAfterSelectProjectingCollection()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var query = session.Query<Entity>()
				.Select(x => new EntityDto
				{
					Id = x.Id,
					ChildNames = x.Children.Select(c => c.Name)
				});

			var count = query.Count();

			Assert.That(count, Is.EqualTo(1));
		}
	}
}
