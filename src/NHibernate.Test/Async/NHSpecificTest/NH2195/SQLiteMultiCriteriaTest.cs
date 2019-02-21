﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Linq;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Multi;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2195
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class SQLiteMultiCriteriaTestAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession session = this.OpenSession())
			{
				DomainClass entity = new DomainClass();
				entity.Id = 1;
				entity.StringData = "John Doe";
				entity.IntData = 1;
				session.Save(entity);

				entity = new DomainClass();
				entity.Id = 2;
				entity.StringData = "Jane Doe";
				entity.IntData = 2;
				session.Save(entity);
				session.Flush();
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();
			using (ISession session = this.OpenSession())
			{
				string hql = "from System.Object";
				session.Delete(hql);
				session.Flush();
			}
		}

		protected override bool AppliesTo(NHibernate.Dialect.Dialect dialect)
		{
			return dialect as SQLiteDialect != null;
		}

		[Test]
		public async Task SingleCriteriaQueriesWithIntsShouldExecuteCorrectlyAsync()
		{
			// Test querying IntData
			using (ISession session = this.OpenSession())
			{
				ICriteria criteriaWithPagination = session.CreateCriteria<DomainClass>();
				criteriaWithPagination.Add(Expression.Le("IntData",2));
				ICriteria criteriaWithRowCount = CriteriaTransformer.Clone(criteriaWithPagination);
				criteriaWithPagination.SetFirstResult(0).SetMaxResults(1);
				criteriaWithRowCount.SetProjection(Projections.RowCountInt64());

				IList<DomainClass> list = await (criteriaWithPagination.ListAsync<DomainClass>());

				Assert.AreEqual(2, await (criteriaWithRowCount.UniqueResultAsync<long>()));
				Assert.AreEqual(1, list.Count);
			}
		}

		[Test]
		public async Task SingleCriteriaQueriesWithStringsShouldExecuteCorrectlyAsync()
		{
			// Test querying StringData
			using (ISession session = this.OpenSession())
			{
				ICriteria criteriaWithPagination = session.CreateCriteria<DomainClass>();
				criteriaWithPagination.Add(Expression.Like("StringData", "%Doe%"));
				ICriteria criteriaWithRowCount = CriteriaTransformer.Clone(criteriaWithPagination);
				criteriaWithPagination.SetFirstResult(0).SetMaxResults(1);
				criteriaWithRowCount.SetProjection(Projections.RowCountInt64());

				IList<DomainClass> list = await (criteriaWithPagination.ListAsync<DomainClass>());

				Assert.AreEqual(2, await (criteriaWithRowCount.UniqueResultAsync<long>()));
				Assert.AreEqual(1, list.Count);
			}
		}

		[Test]
		public async Task QueryBatchWithIntsShouldExecuteCorrectlyAsync()
		{
			// Test querying IntData
			using (var session = OpenSession())
			{
				var criteriaWithPagination = session.CreateCriteria<DomainClass>();
				criteriaWithPagination.Add(Restrictions.Le("IntData", 2));
				var criteriaWithRowCount = CriteriaTransformer.Clone(criteriaWithPagination);
				criteriaWithPagination.SetFirstResult(0).SetMaxResults(1);
				criteriaWithRowCount.SetProjection(Projections.RowCountInt64());

				var multi = session.CreateQueryBatch();
				multi.Add<DomainClass>(criteriaWithPagination);
				multi.Add<long>(criteriaWithRowCount);

				var numResults = (await (multi.GetResultAsync<long>(1, CancellationToken.None))).Single();
				var list = await (multi.GetResultAsync<DomainClass>(0, CancellationToken.None));

				Assert.That(numResults, Is.EqualTo(2));
				Assert.That(list.Count, Is.EqualTo(1));
				Assert.That(await (criteriaWithRowCount.UniqueResultAsync<long>()), Is.EqualTo(2));
			}
		}

		[Test]
		public async Task QueryBatchWithStringsShouldExecuteCorrectlyAsync()
		{
			// Test querying StringData
			using (var session = OpenSession())
			{
				var criteriaWithPagination = session.CreateCriteria<DomainClass>();
				criteriaWithPagination.Add(Restrictions.Like("StringData", "%Doe%"));
				var criteriaWithRowCount = CriteriaTransformer.Clone(criteriaWithPagination);
				criteriaWithPagination.SetFirstResult(0).SetMaxResults(1);
				criteriaWithRowCount.SetProjection(Projections.RowCountInt64());

				var multi = session.CreateQueryBatch();
				multi.Add<DomainClass>(criteriaWithPagination);
				multi.Add<long>(criteriaWithRowCount);

				var numResults = (await (multi.GetResultAsync<long>(1, CancellationToken.None))).Single();
				var list = await (multi.GetResultAsync<DomainClass>(0, CancellationToken.None));

				Assert.That(numResults, Is.EqualTo(2));
				Assert.That(list.Count, Is.EqualTo(1));
				Assert.That(await (criteriaWithRowCount.UniqueResultAsync<long>()), Is.EqualTo(2));
			}
		}
	}
}
