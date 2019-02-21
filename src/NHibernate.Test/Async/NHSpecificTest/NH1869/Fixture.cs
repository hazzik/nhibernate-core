﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Multi;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1869
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		private Keyword _keyword;

		protected override bool AppliesTo(Engine.ISessionFactoryImplementor factory)
		{
		   return factory.ConnectionProvider.Driver.SupportsMultipleQueries;
		}

		protected override void OnTearDown()
		{
			using (var session = Sfi.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from NodeKeyword");
				session.Delete("from Keyword");

				transaction.Commit();
			}
		}

		[Test]
		public async Task TestWithQueryBatchAsync()
		{
			using (var session = Sfi.OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				_keyword = new Keyword();
				await (session.SaveAsync(_keyword));

				var nodeKeyword = new NodeKeyword
				{
					NodeId = 1,
					Keyword = _keyword
				};
				await (session.SaveAsync(nodeKeyword));

				await (transaction.CommitAsync());
			}

			using (var session = Sfi.OpenSession())
			{
				var result = await (GetResultWithQueryBatchAsync(session));
				Assert.That(await (result.GetResultAsync<NodeKeyword>(0, CancellationToken.None)), Has.Count.EqualTo(1));
				Assert.That(await (result.GetResultAsync<NodeKeyword>(1, CancellationToken.None)), Has.Count.EqualTo(1));
			}
		}

		private async Task<IQueryBatch> GetResultWithQueryBatchAsync(ISession session, CancellationToken cancellationToken = default(CancellationToken))
		{
			var query1 = session.CreateQuery("from NodeKeyword nk");
			var query2 = session.CreateQuery("from NodeKeyword nk");

			var multi = session.CreateQueryBatch();
			multi.Add<NodeKeyword>(query1).Add<NodeKeyword>(query2);
			await (multi.ExecuteAsync(cancellationToken));
			return multi;
		}
	}
}
