using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Transaction;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1369
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		// NH-3870: when the underlying ADO.NET Rollback() call fails with a non-HibernateException,
		// AdoTransaction.Rollback() wraps and re-throws the error without ever calling Dispose(), so the
		// wrapped DbTransaction (and everything it references, including the DbConnection) is leaked until
		// the finalizer eventually runs.
		[Test]
		public void FailedRollbackStillDisposesDbTransaction()
		{
			using var session = OpenSession();
			var transaction = session.BeginTransaction();

			var transField = typeof(AdoTransaction).GetField("trans", BindingFlags.NonPublic | BindingFlags.Instance);
			var realDbTransaction = (DbTransaction) transField.GetValue(transaction);

			var faultyDbTransaction = new RollbackFailsDbTransaction(realDbTransaction);
			transField.SetValue(transaction, faultyDbTransaction);

			try
			{
				Assert.That(() => transaction.Rollback(), Throws.TypeOf<TransactionException>());

				Assert.That(
					faultyDbTransaction.WasDisposed,
					Is.True,
					"AdoTransaction.Rollback should dispose the DbTransaction even when the underlying " +
					"Rollback() call fails, otherwise the DbTransaction is leaked.");
			}
			finally
			{
				// Clean up the real underlying transaction, which was never rolled back or disposed since
				// our faulty wrapper intercepted the call. It may already have been aborted at the
				// connection level as a side effect of the failed AdoTransaction.Rollback() call, so this
				// is best effort only and must not hide the actual assertion result above.
				try
				{
					realDbTransaction.Rollback();
				}
				catch (Exception)
				{
					// Ignored: already aborted.
				}
				finally
				{
					realDbTransaction.Dispose();
				}
			}
		}

		private class RollbackFailsDbTransaction : DbTransaction
		{
			private readonly DbTransaction _inner;

			public RollbackFailsDbTransaction(DbTransaction inner)
			{
				_inner = inner;
			}

			public bool WasDisposed { get; private set; }

			protected override DbConnection DbConnection => _inner.Connection;

			public override IsolationLevel IsolationLevel => _inner.IsolationLevel;

			public override void Commit() => _inner.Commit();

			public override void Rollback() => throw new InvalidOperationException("Simulated rollback failure.");

			protected override void Dispose(bool disposing)
			{
				WasDisposed = true;
				base.Dispose(disposing);
			}
		}
	}
}
