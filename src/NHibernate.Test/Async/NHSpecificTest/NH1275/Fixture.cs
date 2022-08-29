﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Cfg;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.NHSpecificTest.NH1275
{
	using System.Threading.Tasks;
	/// <summary>
	/// http://nhibernate.jira.com/browse/NH-1275
	/// </summary>
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return !string.IsNullOrEmpty(dialect.ForUpdateString);
		}

		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.FormatSql, "false");
		}

		[Test]
		public async Task RetrievingAsync()
		{
			object savedId;
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				A a = new A("hunabKu");
				savedId = await (s.SaveAsync(a));
				await (t.CommitAsync());
			}

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				using (SqlLogSpy sqlLogSpy = new SqlLogSpy())
				{
					await (s.GetAsync<A>(savedId, LockMode.Upgrade));
					string sql = sqlLogSpy.Appender.GetEvents()[0].RenderedMessage;
					Assert.That(sql.IndexOf(Dialect.ForUpdateString, StringComparison.Ordinal), Is.GreaterThan(0));
				}
				s.Clear();
				using (SqlLogSpy sqlLogSpy = new SqlLogSpy())
				{
					await (s.GetAsync<A>(typeof(A).FullName, savedId, LockMode.Upgrade));
					string sql = sqlLogSpy.Appender.GetEvents()[0].RenderedMessage;
					Assert.That(sql.IndexOf(Dialect.ForUpdateString, StringComparison.Ordinal), Is.GreaterThan(0));
				}
				using (SqlLogSpy sqlLogSpy = new SqlLogSpy())
				{
					await (s.CreateQuery("from A a where a.Id= :pid").SetLockMode("a", LockMode.Upgrade).SetParameter("pid", savedId).
							UniqueResultAsync<A>());
					string sql = sqlLogSpy.Appender.GetEvents()[0].RenderedMessage;
					Assert.That(sql.IndexOf(Dialect.ForUpdateString, StringComparison.Ordinal), Is.GreaterThan(0));
				}
				await (t.CommitAsync());
			}

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.DeleteAsync("from A"));
				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task LokingAsync()
		{
			object savedId;
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				A a = new A("hunabKu");
				savedId = await (s.SaveAsync(a));
				await (t.CommitAsync());
			}

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				A a = await (s.GetAsync<A>(savedId));
				using (SqlLogSpy sqlLogSpy = new SqlLogSpy())
				{
					await (s.LockAsync(a, LockMode.Upgrade));
					string sql = sqlLogSpy.Appender.GetEvents()[0].RenderedMessage;
					Assert.That(sql.IndexOf(Dialect.ForUpdateString, StringComparison.Ordinal), Is.GreaterThan(0));
				}
				await (t.CommitAsync());
			}

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.DeleteAsync("from A"));
				await (t.CommitAsync());
			}
		}
	}
}
