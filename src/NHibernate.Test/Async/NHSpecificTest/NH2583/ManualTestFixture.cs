﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2583
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class ManualTestFixtureAsync : BugTestCase
	{
		/// <summary>
		/// This setup is used in most tests in here - but not all; and we might want to
		/// twist it for special tests. Therefore, the OnSetUp mechanism is not used.
		/// </summary>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		private async Task StandardSetUpAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var session = OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					var i1001 = new MyRef1 { Id = 1001, I1 = null, I2 = 101 };
					var i1101 = new MyRef1 { Id = 1101, I1 = null, I2 = 111 };
					var i1002 = new MyRef1 { Id = 1002, I1 = 12, I2 = 102 };
					var i1003 = new MyRef1 { Id = 1003, I1 = 13, I2 = 103 };
					await (session.SaveAsync(i1001, cancellationToken));
					await (session.SaveAsync(i1101, cancellationToken));
					await (session.SaveAsync(i1002, cancellationToken));
					await (session.SaveAsync(i1003, cancellationToken));

					var j2001 = new MyRef2 { Id = 2001, J1 = null, J2 = 201 };
					var j2002 = new MyRef2 { Id = 2002, J1 = 22, J2 = 202 };
					var j2003 = new MyRef2 { Id = 2003, J1 = null, J2 = 203 };
					await (session.SaveAsync(j2001, cancellationToken));
					await (session.SaveAsync(j2002, cancellationToken));
					await (session.SaveAsync(j2003, cancellationToken));

					var b10 = new MyBO { Id = 10, Name = "1:1001,o1:NULL,2:NULL", BO1 = i1001, OtherBO1 = null, BO2 = null };
					var b11 = new MyBO { Id = 11, Name = "1:1001,o1:1101,2:NULL", BO1 = i1001, OtherBO1 = i1101, BO2 = null };
					var b20 = new MyBO { Id = 20, Name = "1:1002,o1:NULL,2:2002", BO1 = i1002, OtherBO1 = null, BO2 = j2002 };
					var b30 = new MyBO { Id = 30, Name = "1:NULL,o1:NULL,2:2003", BO1 = null, OtherBO1 = null, BO2 = j2003 };
					await (session.SaveAsync(b10, cancellationToken));
					await (session.SaveAsync(b11, cancellationToken));
					await (session.SaveAsync(b20, cancellationToken));
					await (session.SaveAsync(b30, cancellationToken));
					await (tx.CommitAsync(cancellationToken));
				}
			}
		}

		[Test]
		public async Task OrWithTrueShouldBeEqualToTrueAsync()
		{
			await (StandardSetUpAsync());

			int compareCt;
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>();
				// 3.1.0/2011-03-19: OK - select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_
				var resultList = await (result.ToListAsync());
				compareCt = resultList.Count;
				Assert.IsTrue(compareCt > 0);
			}
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>()
					.Where(bo => true);
				// 3.1.0/2011-03-19: OK - exec sp_executesql N'select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_ where @p0=1',N'@p0 bit',@p0=1
				var resultList = await (result.ToListAsync());
				Assert.AreEqual(compareCt, resultList.Count);
			}
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>()
					.Where(bo =>
							bo.BO1 != null && bo.BO1.I2 == 101
							|| true
							);
				// 3.1.0/2011-03-19: WRONG - exec sp_executesql N'select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_, MyRef1 myref1x1_ where mybo0_.BO1Key=myref1x1_.Id and ((mybo0_.BO1Key is not null) and myref1x1_.I2=@p0 or @p1=1)',N'@p0 int,@p1 bit',@p0=101,@p1=1
				var resultList = await (result.ToListAsync());
				Assert.AreEqual(compareCt, resultList.Count);
			}
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>()
					.Where(bo =>
							bo.BO1 != null && bo.BO1.I2 == 101
							|| bo.Id == bo.Id + 0
							);
				// 3.1.0/2011-03-19: WRONG - exec sp_executesql N'select mybo0_.Id as Id0_, mybo0_.Name as Name0_, mybo0_.BO1Key as BO3_0_, mybo0_.OtherBO1Key as OtherBO4_0_, mybo0_.BO2Key as BO5_0_ from MyBO mybo0_, MyRef1 myref1x1_ where mybo0_.BO1Key=myref1x1_.Id and ((mybo0_.BO1Key is not null) and myref1x1_.I2=@p0 or mybo0_.Id=mybo0_.Id+@p1)',N'@p0 int,@p1 int',@p0=101,@p1=0
				var resultList = await (result.ToListAsync());
				Assert.AreEqual(compareCt, resultList.Count);
			}
		}

		[Test]
		public async Task OrAndNavigationsShouldUseOuterJoinsAsync()
		{
			await (StandardSetUpAsync());

			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>()
					.Where(bo =>
							bo.BO1 != null && bo.BO1.I2 == 101
							// || bo.BO2 != null && bo.BO2.J2 == 203 - is added below!
							);
				var resultList = await (result.ToListAsync());
				Assert.IsTrue(resultList.Count > 0);
			}
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>()
					.Where(bo =>
							bo.BO1 != null && bo.BO1.I2 == 101
							|| bo.BO2 != null && bo.BO2.J2 == 203
							);
				var resultList = await (result.ToListAsync());
				Assert.IsTrue(resultList.Count > 0);
			}
			using (var session = OpenSession())
			{
				var result = session.Query<MyBO>()
					.Where(bo =>
							bo.BO1.I2 == 101 && bo.BO1 != null
							|| bo.BO2.J2 == 203 && bo.BO2 != null
							);
				var resultList = await (result.ToListAsync());
				Assert.IsTrue(resultList.Count > 0);
			}
		}

		[Test]
		public async Task OrShouldBeCompatibleWithAdditionForNullReferencesAsync()
		{
			// This tests against the "outer join anomaly" of (||-3) semantics.
			await (StandardSetUpAsync());

			using (var session = OpenSession())
			{
				List<MyBO> leftResult;
				List<MyBO> rightResult;
				List<MyBO> orResult;
				// Gets translated to SQL, where it can be null.
#pragma warning disable CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
				TestCoreOrShouldBeCompatibleWithSum(session,
					bo => bo.BO1.I2 == null,
					bo => bo.BO2.J2 == null,
					bo => bo.BO1.I2 == null || bo.BO2.J2 == null, out leftResult, out rightResult, out orResult);
#pragma warning restore CS0472 // The result of the expression is always the same since a value of this type is never equal to 'null'
				//Assert.AreEqual(0, leftResult.Count);
				//Assert.AreEqual(0, rightResult.Count);
				//Assert.AreEqual(0, orResult.Count);
			}
		}

		private static void TestCoreOrShouldBeCompatibleWithSum(ISession session,
			Expression<Func<MyBO, bool>> left,
			Expression<Func<MyBO, bool>> right,
			Expression<Func<MyBO, bool>> both,
			out List<MyBO> leftResult,
			out List<MyBO> rightResult,
			out List<MyBO> orResult)
		{
			leftResult = session.Query<MyBO>()
				.Where(left
				).ToList();

			rightResult = session.Query<MyBO>()
				.Where(right
				).ToList();

			orResult = session.Query<MyBO>()
				.Where(both
				).ToList();
			Assert.IsTrue(orResult.Count <= leftResult.Count + rightResult.Count);
		}

		[Test]
		public async Task OrShouldBeCompatibleWithAdditionForNonNullReferencesAsync()
		{
			// This tests one case of existing references - so that we do not fall
			// into trap of testing only empty results that might be empty for other reasons.
			await (StandardSetUpAsync());

			using (var session = OpenSession())
			{
				List<MyBO> leftResult;
				List<MyBO> rightResult;
				List<MyBO> orResult;
				TestCoreOrShouldBeCompatibleWithSum(session,
					bo => bo.BO1.I1 == null,
					bo => bo.BO2.J1 == null,
					bo => bo.BO1.I1 == null || bo.BO2.J1 == null, out leftResult, out rightResult, out orResult);
				Assert.That(() => leftResult.Count, Is.GreaterThan(0));
				Assert.That(() => rightResult.Count, Is.GreaterThan(0));
				Assert.That(() => orResult.Count, Is.GreaterThan(0));
			}
		}

		[Test, Explicit("Exploratory Test")]
		public async Task NHibernateLinqExploratoryTestWhichProvesNothingAsync()
		{
			await (StandardSetUpAsync());

			using (var session = OpenSession())
			{
				//var result = session.Query<MyBO>()
				//    .Where(bo =>
				//        // ReSharper disable EqualExpressionComparison
				//            bo.BO1.I2 == bo.BO1.I2
				//    // ReSharper restore EqualExpressionComparison
				//            );
				//result.ToList();

				var result = from r in session.Query<MyRef1>()
							 orderby (r.Id == 1101 || r.Id == 1102 ? r.Id - 1000 : r.Id)
							 select (r.Id == 1101 || r.Id == 1102 ? r.Id + 1000 : r.Id);
				CollectionAssert.AreEqual(new[] { 2101, 1001, 1002, 1003 }, await (result.ToListAsync()));

				var someBos = from r in session.Query<MyBO>()
							  where r.BO1.BO2.J2 == 201
							  select r;
				Assert.IsTrue((await (someBos.ToListAsync())).Count == 0);

				var someBos1 = from r in session.Query<MyBO>()
							   where (r.BO1.BO2 == null ? r.BO2 : r.BO1.BO2).J2 == 201
							   select r.Id;
				Assert.That(() => someBos1.ToListAsync(), Is.EquivalentTo(new[] { 10 }));
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			{
				using (var tx = session.BeginTransaction())
				{
					foreach (var bo in session.Query<MyBO>())
					{
						bo.LeftSon = null;
						bo.RightSon = null;
					}
					session.Flush();
					DeleteAll<MyBO>(session);
					DeleteAll<MyRef1>(session);
					DeleteAll<MyRef2>(session);
					DeleteAll<MyRef3>(session);
					tx.Commit();
				}
			}
		}

		private static void DeleteAll<T>(ISession session)
		{
			session.CreateQuery("delete from " + typeof(T).Name).ExecuteUpdate();
		}
	}
}
