using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1034
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.ConnectionDriver, typeof(AdjustingDriver).AssemblyQualifiedName);
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(rc =>
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

			session.Save(new Entity { Name = "Widget" });

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
		public void DriverAdjustmentIsReflectedInLoggedSql()
		{
			using var log = new SqlLogSpy();
			using var session = OpenSession();

			var results = session.CreateQuery("from Entity").List<Entity>();

			Assert.That(results, Has.Count.EqualTo(1));
			Assert.That(
				log.GetWholeLog(),
				Does.Contain(AdjustingDriver.AdjustmentMarker),
				"The logged SQL should reflect the driver's AdjustCommand modification, since the " +
				"command adjustment is meant to be visible in the log.");
		}
	}
}
