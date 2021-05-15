﻿using System;
using System.Transactions;
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

		protected override string CacheConcurrencyStrategy => "read-write";

		[Test]
		public void CacheTest()
		{
			// create transaction (rollback at the end)
			var comments = Guid.NewGuid().ToString("N");
			
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new SpecificVehicle {Id = 358});
				transaction.Commit();
			}

			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
			{
				// request #1 - load
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload version
					var entity = session.Get<SpecificVehicle>(358);
					// assert 
					Assert.That(entity, Is.Not.Null);

					// commit
					transaction.Commit();


					Assert.That(Sfi.Statistics.SecondLevelCachePutCount, Is.EqualTo(1));
				}

				// request #2 - publish
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload
					var entity = session.Load<SpecificVehicle>(358);
					// update
					entity.Comments = comments;
					// commit
					transaction.Commit();

					Assert.That(Sfi.Statistics.SecondLevelCacheHitCount, Is.EqualTo(2));
					Assert.That(Sfi.Statistics.SecondLevelCachePutCount, Is.EqualTo(2));
				}

				// request #3 - reload
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload version
					var entity = session.Get<SpecificVehicle>(358);
					// assert 
					Assert.IsTrue(entity.Comments == comments, "The property was not updated");
					// commit
					transaction.Commit();

					Assert.That(Sfi.Statistics.SecondLevelCacheHitCount, Is.EqualTo(3));
					Assert.That(Sfi.Statistics.SecondLevelCachePutCount, Is.EqualTo(2));
				}
			}

			// request #4 - load after rollback
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
			{
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload version
					var entity = session.Get<SpecificVehicle>(358);
					// assert 
					Assert.IsTrue(
						entity.Comments != comments,
						"The property was updated after rollback"); // THIS ASSERTION FAILS
					// commit
					transaction.Commit();

					Assert.That(Sfi.Statistics.SecondLevelCacheHitCount, Is.EqualTo(4));
					Assert.That(Sfi.Statistics.SecondLevelCachePutCount, Is.EqualTo(2));
				}
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from SpecificVehicle").ExecuteUpdate();
				transaction.Commit();
			}

			base.OnTearDown();
		}
	}
}
