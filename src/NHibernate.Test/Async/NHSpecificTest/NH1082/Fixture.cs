﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1082
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		[Test]
		public async Task ExceptionsInBeforeTransactionCompletionAbortTransactionAsync()
		{
			var c = new C {ID = 1, Value = "value"};

			var sessionInterceptor = new SessionInterceptorThatThrowsExceptionAtBeforeTransactionCompletion();
			using (var s = Sfi.WithOptions().Interceptor(sessionInterceptor).OpenSession())
			using (var t = s.BeginTransaction())
			{
				await (s.SaveAsync(c));

				Assert.ThrowsAsync<BadException>(() => t.CommitAsync());
			}

			using (ISession s = Sfi.OpenSession())
			{
				var objectInDb = await (s.GetAsync<C>(1));
				Assert.IsNull(objectInDb);
			}
		}

		[Test]
		public async Task ExceptionsInTransactionSynchronizationBeforeTransactionCompletionAbortTransactionAsync()
		{
			var c = new C { ID = 1, Value = "value" };

			var synchronization = new TransactionSynchronizationThatThrowsExceptionAtBeforeTransactionCompletion();
			using (ISession s = Sfi.OpenSession())
			using (ITransaction t = s.BeginTransaction())
			{
				t.RegisterSynchronization(synchronization);

				await (s.SaveAsync(c));

				Assert.ThrowsAsync<BadException>(() => t.CommitAsync());
			}

			using (ISession s = Sfi.OpenSession())
			{
				var objectInDb = await (s.GetAsync<C>(1));
				Assert.IsNull(objectInDb);
			}
		}
	}
}
