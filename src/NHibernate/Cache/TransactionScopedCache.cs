using System;
using System.Collections.Concurrent;
using System.Transactions;

namespace NHibernate.Cache
{
	public partial class TransactionScopedCache : CacheBase
	{
		private readonly CacheBase _rootCache;
		private readonly ConcurrentDictionary<System.Transactions.Transaction, ActionRecordingCache> _caches = new ConcurrentDictionary<System.Transactions.Transaction, ActionRecordingCache>();

		public TransactionScopedCache(CacheBase rootCache)
		{
			_rootCache = rootCache;
			Timeout = rootCache.Timeout;
			RegionName = rootCache.RegionName;
		}

		public override object Get(object key)
		{
			return GetCache().Get(key);
		}

		public override void Put(object key, object value)
		{
			GetCache().Put(key, value);
		}

		public override void Remove(object key)
		{
			GetCache().Remove(key);
		}

		public override void Clear()
		{
			GetCache().Clear();
		}

		public override void Destroy()
		{
			GetCache().Destroy();
		}

		public override object Lock(object key)
		{
			return GetCache().Lock(key);
		}

		public override void Unlock(object key, object lockValue)
		{
			GetCache().Unlock(key, lockValue);
		}

		public override long NextTimestamp()
		{
			return GetCache().NextTimestamp();
		}

		public override int Timeout { get; }
		public override string RegionName { get; }

		private CacheBase GetCache()
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
				_cache._caches.TryRemove(_transaction, out _);
			}

			void IEnlistmentNotification.InDoubt(Enlistment enlistment)
			{
			}

			void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
			{
				try
				{
					if (_cache._caches.TryGetValue(_transaction, out var cache))
					{
						cache.Replay();
					}

					preparingEnlistment.Prepared();
				}
				catch (Exception exception)
				{
					preparingEnlistment.ForceRollback(exception);
				}
			}

			void IEnlistmentNotification.Rollback(Enlistment enlistment)
			{
				_cache._caches.TryRemove(_transaction, out _);
			}
		}
	}
}
