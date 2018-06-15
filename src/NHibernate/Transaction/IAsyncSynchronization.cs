using System.Threading;
using System.Threading.Tasks;

namespace NHibernate.Transaction
{
	public interface IAsyncSynchronization : ISynchronization
	{
		Task BeforeCompletionAsync(CancellationToken cancellationToken = default(CancellationToken));
		Task AfterCompletionAsync(bool success, CancellationToken cancellationToken = default(CancellationToken));
	}
}