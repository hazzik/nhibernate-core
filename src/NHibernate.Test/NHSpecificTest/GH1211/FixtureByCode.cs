using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1211
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<EntityA>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
				});

			mapper.Class<EntityB>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new EntityA { Id = 1, Name = "a1" });
			session.Save(new EntityB { Id = 1, Name = "b1" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from EntityA").ExecuteUpdate();
			session.CreateQuery("delete from EntityB").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ReturnAliasesAreNotNullWhenNoSelectClauseSpecified()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var query = session.CreateQuery("from EntityA as a, EntityB as b");

			Assert.That(query.ReturnAliases, Is.Not.Null, "ReturnAliases should not be null when no SELECT clause is specified");
			Assert.That(query.ReturnAliases, Is.EqualTo(new[] { "a", "b" }));

			transaction.Commit();
		}
	}
}
