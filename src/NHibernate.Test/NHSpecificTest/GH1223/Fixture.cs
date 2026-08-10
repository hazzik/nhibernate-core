using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1223
{
	// NH-2396 / GH-1223: ISession.CancelQuery() does not cancel a native SQL query.
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new Mapping.ByCode.ModelMapper();
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is Dialect.PostgreSQL83Dialect;
		}

		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.ConnectionDriver, typeof(CancelSpyDriver).AssemblyQualifiedName);
		}

		protected override void OnTearDown()
		{
			CancelSpyDriver.Reset();
		}

		[Test]
		public void CancelQueryReachesNativeSqlUpdateCommand()
		{
			using (var session = OpenSession())
			{
				CancelSpyDriver.Session = session;

				// session.CancelQuery() is invoked from within the command's own ExecuteNonQuery(),
				// which is equivalent to (but deterministic, unlike) another thread calling it while
				// the command is running. NativeSQLQueryPlan.PerformExecuteUpdate() prepares its
				// command with Batcher.PrepareCommand(), which, unlike Batcher.PrepareQueryCommand()
				// used for HQL/Criteria/LINQ queries, never records the command as the batcher's
				// "last query". So CancelQuery() never reaches the command running this native SQL
				// query.
				session.CreateSQLQuery("select 1").ExecuteUpdate();
			}

			Assert.That(
				CancelSpyDriver.CancelCalled,
				Is.True,
				"ISession.CancelQuery() did not reach the DbCommand executing the native SQL query.");
		}
	}
}
