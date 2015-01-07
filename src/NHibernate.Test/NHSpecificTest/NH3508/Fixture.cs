using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Transactions;
using NHibernate.Test.NHSpecificTest.NH1584;
using NUnit.Framework;
using Configuration = NHibernate.Cfg.Configuration;

namespace NHibernate.Test.NHSpecificTest.NH3508
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Cfg.Environment.GenerateStatistics, "true");
		}

		protected override string CacheConcurrencyStrategy
		{
			get { return "read-write"; }
		}

		[Test]
		public void CacheTest()
		{
			// create transaction (rollback at the end)
			var comments = Guid.NewGuid().ToString("N");
			
			using (var session = sessions.OpenSession())
			{
				// create tx
				using (var transaction = session.BeginTransaction())
				{
					session.Save(new SpecificVehicle {Id = 358});
					// commit
					transaction.Commit();
				}
			}

			using (var scope=new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
			{
				// request #1 - load
				// open another session to force using 2nd level cache values
				using (var session = sessions.OpenSession())
				{
					// create tx
					using (var transaction = session.BeginTransaction())
					{
						// reload version
						var entity = session.Get<SpecificVehicle>(358);
						// assert 
						Assert.IsNotNull(entity);
						
						// commit
						transaction.Commit();

					
						Assert.AreEqual(1, sessions.Statistics.SecondLevelCachePutCount);

					}
				}
				// request #2 - publish
				// open another session to force using 2nd level cache values
				using (var session = sessions.OpenSession())
				{
					// create tx
					using (var transaction = session.BeginTransaction())
					{
						// reload
						var entity = session.Load<SpecificVehicle>(358);
						// update
						entity.Comments = comments;
						// commit
						transaction.Commit();

						Assert.AreEqual(2, sessions.Statistics.SecondLevelCacheHitCount);
						Assert.AreEqual(2, sessions.Statistics.SecondLevelCachePutCount);
						
					}
				}
				// request #3 - reload
				// open another session to force using 2nd level cache values
				using (var session = sessions.OpenSession())
				{
					// create tx
					using (var transaction = session.BeginTransaction())
					{
						// reload version
						var entity = session.Get<SpecificVehicle>(358);
						// assert 
						Assert.IsTrue(entity.Comments == comments, "The property was not updated");
						// commit
						transaction.Commit();

						Assert.AreEqual(3, sessions.Statistics.SecondLevelCacheHitCount);
						Assert.AreEqual(2, sessions.Statistics.SecondLevelCachePutCount);
					}
				}
			}
			// request #4 - load after rollback
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
			{
				// open another session to force using 2nd level cache values
				using (var session = sessions.OpenSession())
				{
					// create tx
					using (var transaction = session.BeginTransaction())
					{
						// reload version
						var entity = session.Get<SpecificVehicle>(358);
						// assert 
						Assert.IsTrue(entity.Comments != comments, "The property was updated after rollback"); // THIS ASSERTION FAILS
						// commit
						transaction.Commit();

						Assert.AreEqual(4, sessions.Statistics.SecondLevelCacheHitCount);
						Assert.AreEqual(2, sessions.Statistics.SecondLevelCachePutCount);
					}
				}
			}

		}

		protected override void OnTearDown()
		{
			using (var session = sessions.OpenSession())
			{
				// create tx
				using (var transaction = session.BeginTransaction())
				{
					session.Delete("from SpecificVehicle");
					// commit
					transaction.Commit();
				}
			}
			base.OnTearDown();
		}
	}
}
