using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Transactions;

namespace NHibernate.Cache
{
	public class TransactionScopedCache : ICache
	{
		private readonly ICache _rootCache;
		private ConcurrentDictionary<System.Transactions.Transaction, ActionRecordingCache> _caches = new ConcurrentDictionary<System.Transactions.Transaction, ActionRecordingCache>();

		public TransactionScopedCache(ICache rootCache)
		{
			_rootCache = rootCache;
			Timeout = rootCache.Timeout;
			RegionName = rootCache.RegionName;
		}

		public object Get(object key)
		{
			return GetCache().Get(key);
		}

		public void Put(object key, object value)
		{
			GetCache().Put(key, value);
		}

		public void Remove(object key)
		{
			GetCache().Remove(key);
		}

		public void Clear()
		{
			GetCache().Clear();
		}

		public void Destroy()
		{
			GetCache().Destroy();
		}

		public void Lock(object key)
		{
			GetCache().Lock(key);
		}

		public void Unlock(object key)
		{
			GetCache().Unlock(key);
		}

		public long NextTimestamp()
		{
			return GetCache().NextTimestamp();
		}

		public int Timeout { get; private set; }
		public string RegionName { get; private set; }

		private ICache GetCache()
		{
			var currentTx = System.Transactions.Transaction.Current;
			if (currentTx != null)
			{
				return _caches.GetOrAdd(currentTx, tx =>
				{
					currentTx.EnlistVolatile(new EnlistmentNotification(this, tx), EnlistmentOptions.None);
					return new ActionRecordingCache(RegionName, _rootCache);
				});
			}
			return _rootCache;
		}

		public class EnlistmentNotification : IEnlistmentNotification
		{
			private readonly TransactionScopedCache _cache;
			private readonly System.Transactions.Transaction _transaction;

			internal EnlistmentNotification(TransactionScopedCache cache, System.Transactions.Transaction transaction)
			{
				_cache = cache;
				_transaction = transaction;
			}

			void IEnlistmentNotification.Commit(Enlistment enlistment)
			{
				ActionRecordingCache cache;
				_cache._caches.TryRemove(_transaction, out cache);
			}

			void IEnlistmentNotification.InDoubt(Enlistment enlistment)
			{

			}

			void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
			{
				try
				{
					ActionRecordingCache cache;
					if (_cache._caches.TryGetValue(_transaction, out cache))
					{
						cache.Replay();
					}
					preparingEnlistment.Prepared();
				}
				catch(Exception exception)
				{
					preparingEnlistment.ForceRollback(exception);
				}
			}

			void IEnlistmentNotification.Rollback(Enlistment enlistment)
			{
				ActionRecordingCache cache;
				_cache._caches.TryRemove(_transaction, out cache);
			}
		}

	}
}
