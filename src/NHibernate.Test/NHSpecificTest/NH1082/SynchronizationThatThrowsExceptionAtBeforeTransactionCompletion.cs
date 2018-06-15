using System.Threading;
using System.Threading.Tasks;
using NHibernate.Transaction;

namespace NHibernate.Test.NHSpecificTest.NH1082
{
	public class SynchronizationThatThrowsExceptionAtBeforeTransactionCompletion : IAsyncSynchronization
	{
		public void BeforeCompletion()
		{
			throw new BadException();
		}

		public void AfterCompletion(bool success)
		{
		}

		public Task BeforeCompletionAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			return Task.FromException(new BadException());
		}

		public Task AfterCompletionAsync(bool success, CancellationToken cancellationToken = default(CancellationToken))
		{
			return Task.CompletedTask;
		}
	}
}