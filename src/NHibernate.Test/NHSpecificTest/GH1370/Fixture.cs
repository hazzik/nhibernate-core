using NHibernate.Id.Enhanced;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1370
{
	[TestFixture]
	public class Fixture : TestCase
	{
		protected override string[] Mappings => new[] { "NHSpecificTest.GH1370.Mappings.hbm.xml" };

		protected override string MappingsAssembly => "NHibernate.Test";

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();
			session.CreateQuery("delete from System.Object").ExecuteUpdate();
			transaction.Commit();
		}

		// Simulates two different processes sharing the same hi/lo table in the database.
		// Each process has its own in-memory HiLoOptimizer, but the optimizer must never hand out
		// an identifier that another process could already have handed out from the same table.
		[Test]
		public void TwoProcessesDoNotProduceDuplicateIdentifiers()
		{
			var persister = Sfi.GetEntityPersister(typeof(Entity).FullName);
			var generator = (TableGenerator) persister.IdentifierGenerator;
			var optimizer = (OptimizerFactory.HiLoOptimizer) generator.Optimizer;
			var incrementSize = optimizer.IncrementSize;

			using var processBFactory = cfg.BuildSessionFactory();

			// Process A exhausts three full "hi" buckets on its own, so its optimizer is primed to
			// roll over to a new bucket on its very next call.
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				for (var i = 0; i < incrementSize * 3; i++)
				{
					session.Save(new Entity { Name = "A" + i });
				}

				transaction.Commit();
			}

			// Process B starts fresh and fetches the next "hi" value from the shared database table.
			long processBId;
			using (var session = processBFactory.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = new Entity { Name = "B" };
				session.Save(entity);
				processBId = entity.Id;
				transaction.Commit();
			}

			// Process A now rolls over to a new bucket. Because of the reported defect, it fetches a
			// new "hi" value from the database but keeps using its stale low value, re-issuing an
			// identifier already handed out to process B.
			long processARolloverId;
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = new Entity { Name = "A-rollover" };
				session.Save(entity);
				processARolloverId = entity.Id;
				transaction.Commit();
			}

			Assert.That(
				processARolloverId,
				Is.Not.EqualTo(processBId),
				"HiLoOptimizer re-issued an identifier already handed out to a different process after a bucket rollover.");
		}
	}
}
