using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1247
{
	// NH-2878 (GH-1247): the alias of a table function used in the FROM clause of a formula
	// gets incorrectly prefixed with the placeholder for the owning entity's table alias,
	// producing invalid SQL such as "from generate_series(1, 10) table0_.gs".
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
					rc.Property(
						x => x.GeneratedCount,
						m =>
						{
							// The "gs" alias here is a local alias for the table function
							// generate_series, not a column of Entity. It must not be
							// prefixed with the entity's table alias.
							m.Formula("(select count(*) from generate_series(1, 10) gs)");
							m.Access(Accessor.ReadOnly);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Entity { Id = 1, Name = "Test" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Entity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanLoadEntityWithTableFunctionFormula()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var entity = session.Get<Entity>(1);

			Assert.That(entity.GeneratedCount, Is.EqualTo(10));

			transaction.Commit();
		}
	}
}
