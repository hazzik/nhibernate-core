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
using System.Text;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2583
{
	using System.Threading.Tasks;
	[TestFixture]
	public class InnerJoinFixtureAsync : BugTestCase
	{
		[Test]
		public async Task OrShouldBeOuterJoinAsync()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				await (session.Query<MyBO>().Where(b => b.BO1.I1 == 1 || b.BO2.J1 == 1).ToListAsync());
				var log = sqlLog.GetWholeLog();
				Assert.AreEqual(2, CountOuterJoins(log));
				Assert.AreEqual(0, CountInnerJoins(log));
			}
		}

		[Test]
		public async Task AndShouldBeInnerJoinAsync()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				await (session.Query<MyBO>().Where(b => b.BO1.I1 == 1 && b.BO2.J1 == 1).ToListAsync());
				var log = sqlLog.GetWholeLog();
				Assert.AreEqual(0, CountOuterJoins(log));
				Assert.AreEqual(2, CountInnerJoins(log));
			}
		}

		[Test]
		public async Task ComparisonToConstantShouldBeInnerJoinAsync()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				await (session.Query<MyBO>().Where(b => b.BO1.I1 == 1).ToListAsync());
				var log = sqlLog.GetWholeLog();
				Assert.AreEqual(0, CountOuterJoins(log));
				Assert.AreEqual(1, CountInnerJoins(log));
			}
		}

		[Test]
		public async Task NotEqualsNullShouldBeInnerJoinAsync()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				await (session.Query<MyBO>().Where(b => b.BO1.BO2 != null).ToListAsync());
				var log = sqlLog.GetWholeLog();
				Assert.AreEqual(0, CountOuterJoins(log));
				Assert.AreEqual(1, CountInnerJoins(log));
			}
		}

		[Test]
		public async Task EqualsNullShouldBeOuterJoinAsync()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var session = OpenSession())
			{
				await (session.Query<MyBO>().Where(b => b.BO1.BO2 == null).ToListAsync());
				var log = sqlLog.GetWholeLog();
				Assert.AreEqual(1, CountOuterJoins(log));
				Assert.AreEqual(0, CountInnerJoins(log));
			}
		}

		private int CountOuterJoins(string log)
		{
			return log.Split(new[] { "left outer join" }, StringSplitOptions.None).Count() - 1;
		}

		private int CountInnerJoins(string log)
		{
			return log.Split(new[] { "inner join" }, StringSplitOptions.None).Count() - 1;
		}
	}
}
