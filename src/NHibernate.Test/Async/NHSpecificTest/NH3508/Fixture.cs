﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Transactions;
using NUnit.Framework;
using Configuration = NHibernate.Cfg.Configuration;

namespace NHibernate.Test.NHSpecificTest.NH3508
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Cfg.Environment.GenerateStatistics, "true");
		}

		protected override string CacheConcurrencyStrategy => "read-write";

		[Test]
		public async Task CacheTestAsync()
		{
			// create transaction (rollback at the end)
			var comments = Guid.NewGuid().ToString("N");
			
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				await (session.SaveAsync(new SpecificVehicle {Id = 358}));
				await (transaction.CommitAsync());
			}

			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
			{
				// request #1 - load
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload version
					var entity = await (session.GetAsync<SpecificVehicle>(358));
					// assert 
					Assert.That(entity, Is.Not.Null);

					// commit
					await (transaction.CommitAsync());


					Assert.That(Sfi.Statistics.SecondLevelCachePutCount, Is.EqualTo(1));
				}

				// request #2 - publish
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload
					var entity = await (session.LoadAsync<SpecificVehicle>(358));
					// update
					entity.Comments = comments;
					// commit
					await (transaction.CommitAsync());

					Assert.That(Sfi.Statistics.SecondLevelCacheHitCount, Is.EqualTo(2));
					Assert.That(Sfi.Statistics.SecondLevelCachePutCount, Is.EqualTo(2));
				}

				// request #3 - reload
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload version
					var entity = await (session.GetAsync<SpecificVehicle>(358));
					// assert 
					Assert.IsTrue(entity.Comments == comments, "The property was not updated");
					// commit
					await (transaction.CommitAsync());

					Assert.That(Sfi.Statistics.SecondLevelCacheHitCount, Is.EqualTo(3));
					Assert.That(Sfi.Statistics.SecondLevelCachePutCount, Is.EqualTo(2));
				}
			}

			// request #4 - load after rollback
			using (new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }, TransactionScopeAsyncFlowOption.Enabled))
			{
				// open another session to force using 2nd level cache values
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// reload version
					var entity = await (session.GetAsync<SpecificVehicle>(358));
					// assert 
					Assert.IsTrue(
						entity.Comments != comments,
						"The property was updated after rollback"); // THIS ASSERTION FAILS
					// commit
					await (transaction.CommitAsync());

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
