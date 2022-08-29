﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Cfg;
using NHibernate.Exceptions;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1813
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.BatchSize, "0");
		}

		[Test]
		public async Task ContainSQLInInsertAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.SaveAsync(new EntityWithUnique { Id = 1, Description = "algo" }));
				await (t.CommitAsync());
			}
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.SaveAsync(new EntityWithUnique { Id = 2, Description = "algo" }));
				var exception = Assert.ThrowsAsync<GenericADOException>(() => t.CommitAsync());
				Assert.That(exception.Message, Does.Contain("INSERT"), "should contain SQL");
				Assert.That(exception.Message, Does.Contain("#2"), "should contain id");
			}
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.CreateQuery("delete from EntityWithUnique").ExecuteUpdateAsync());
				await (t.CommitAsync());
			}
		}

		[Test]
		public async Task ContainSQLInUpdateAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.SaveAsync(new EntityWithUnique { Id = 1, Description = "algo" }));
				await (s.SaveAsync(new EntityWithUnique { Id = 2, Description = "mas" }));
				await (t.CommitAsync());
			}
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				var e = await (s.GetAsync<EntityWithUnique>(2));
				e.Description = "algo";
				var exception = Assert.ThrowsAsync<GenericADOException>(() => t.CommitAsync());
				Assert.That(exception.Message, Does.Contain("UPDATE"), "should contain SQL");
			}
			using (ISession s = OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				await (s.CreateQuery("delete from EntityWithUnique").ExecuteUpdateAsync());
				await (t.CommitAsync());
			}
		}
	}
}
