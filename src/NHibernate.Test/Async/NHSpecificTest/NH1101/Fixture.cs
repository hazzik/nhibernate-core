﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Stat;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1101
{
	using System.Threading.Tasks;
	// http://nhibernate.jira.com/browse/NH-1101
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void Configure(Cfg.Configuration configuration)
		{
			base.Configure(configuration);
			cfg.SetProperty(Cfg.Environment.GenerateStatistics, "true");
		}

		[Test]
		public async Task BehaviorAsync()
		{
			object savedId;
			A a = new A();
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				savedId = await (s.SaveAsync(a));
				await (t.CommitAsync());
			}

			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				a = await (s.GetAsync<A>(savedId));

				IStatistics statistics = Sfi.Statistics;
				statistics.Clear();

				Assert.IsNotNull(a.B); // an instance of B was created
				await (s.FlushAsync());
				await (t.CommitAsync());

				// since we don't change anyproperties in a.B there are no dirty entity to commit
				Assert.AreEqual(0, statistics.PrepareStatementCount);
			}

			// using proxy
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				a = await (s.LoadAsync<A>(savedId));

				IStatistics statistics = Sfi.Statistics;
				statistics.Clear();

				Assert.IsNotNull(a.B); // an instance of B was created
				await (s.FlushAsync());
				await (t.CommitAsync());

				Assert.AreEqual(0, statistics.PrepareStatementCount);
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
