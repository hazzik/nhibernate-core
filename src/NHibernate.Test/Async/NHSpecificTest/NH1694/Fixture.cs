﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1694
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is MsSql2005Dialect;
		}

		private async Task FillDbAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			base.OnSetUp();
			using (ISession session = OpenSession())
			{
				using (ITransaction tran = session.BeginTransaction())
				{
					var newUser = new User();
					var newOrder1 = new Orders { User = newUser, Status = true };
					var newOrder2 = new Orders { User = newUser, Status = true };

					await (session.SaveAsync(newUser, cancellationToken));
					await (session.SaveAsync(newOrder1, cancellationToken));
					await (session.SaveAsync(newOrder2, cancellationToken));

					newUser = new User();
					newOrder1 = new Orders { User = newUser, Status = false };

					await (session.SaveAsync(newUser, cancellationToken));
					await (session.SaveAsync(newOrder1, cancellationToken));

					await (tran.CommitAsync(cancellationToken));
				}
			}
		}

		private async Task CleanupAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			base.OnTearDown();
			using (ISession session = OpenSession())
			{
				using (ITransaction tran = session.BeginTransaction())
				{
					await (session.DeleteAsync("from Orders", cancellationToken));
					await (session.DeleteAsync("from User", cancellationToken));
					await (tran.CommitAsync(cancellationToken));
				}
			}
		}

		[Test]
		public async Task CanOrderByExpressionContainingACommaInAPagedQueryAsync()
		{
			await (FillDbAsync());
			using (ISession session = OpenSession())
			{
				using (ITransaction tran = session.BeginTransaction())
				{
					ICriteria crit = session.CreateCriteria(typeof(User));
					crit.AddOrder(Order.Desc("OrderStatus"));
					crit.AddOrder(Order.Asc("Id"));
					crit.SetMaxResults(10);

					IList<User> list = await (crit.ListAsync<User>());

					Assert.That(list.Count, Is.EqualTo(2));
					Assert.That(list[0].OrderStatus, Is.EqualTo(2));
					Assert.That(list[1].OrderStatus, Is.EqualTo(1));

					await (tran.CommitAsync());
				}
			}
			await (CleanupAsync());
		}
	}
}
