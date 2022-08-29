﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Linq;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2404
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new TestEntity
				{
					Id = 1,
					Name = "Test Entity"
				});

				session.Save(new TestEntity
				{
					Id = 2,
					Name = "Test Entity"
				});

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				transaction.Commit();
			}
		}

		[Test]
		public async Task ProjectionsShouldWorkWithLinqProviderAndFuturesAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var query1 = await ((from entity in session.Query<TestEntity>()
							  select new TestEntityDto { EntityId = entity.Id, EntityName = entity.Name }).ToListAsync());

				Assert.AreEqual(2, query1.Count());

				var query2 = (from entity in session.Query<TestEntity>()
							  select new TestEntityDto { EntityId = entity.Id, EntityName = entity.Name }).ToFuture();

				Assert.AreEqual(2, (await (query2.GetEnumerableAsync())).Count());
			}
		}

		[Test]
		public async Task ProjectionsShouldWorkWithHqlAndFuturesAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var query1 =
					await (session.CreateQuery("select e.Id as EntityId, e.Name as EntityName from TestEntity e").SetResultTransformer(
						Transformers.AliasToBean(typeof(TestEntityDto)))
						.ListAsync<TestEntityDto>());

				Assert.AreEqual(2, query1.Count());

				var query2 =
					session.CreateQuery("select e.Id as EntityId, e.Name as EntityName from TestEntity e").SetResultTransformer(
						Transformers.AliasToBean(typeof(TestEntityDto)))
						.Future<TestEntityDto>();

				Assert.AreEqual(2, (await (query2.GetEnumerableAsync())).Count());
			}
		}
	}
}
