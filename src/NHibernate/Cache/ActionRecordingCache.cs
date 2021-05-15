using System;
using System.Collections.Generic;

namespace NHibernate.Cache
{
	internal partial class ActionRecordingCache : CacheBase
	{
		private readonly CacheBase _innerCache;
		private readonly CacheBase _outerCache;
		private readonly Queue<Action<CacheBase>> _actions;

		public ActionRecordingCache(string regionName, CacheBase outerCache)
			: this(new HashtableCache(regionName), outerCache)
		{
		}

		public ActionRecordingCache(CacheBase innerCache, CacheBase outerCache)
		{
			_innerCache = innerCache;
			_outerCache = outerCache;
			_actions = new Queue<Action<CacheBase>>();
		}

		public override object Get(object key)
		{
			var instance = _innerCache.Get(key);
			if (instance != null)
			{
				return instance;
			}
			return _outerCache.Get(key);
		}

		public override void Put(object key, object value)
		{
			_innerCache.Put(key, value);
			_actions.Enqueue(c => c.Put(key, value));
		}

		public override void Remove(object key)
		{
			_innerCache.Remove(key);
			_actions.Enqueue(c => c.Remove(key));
		}

		public override void Clear()
		{
			_innerCache.Clear();
			_actions.Clear();
			_actions.Enqueue(c => c.Clear());
		}

		public override void Destroy()
		{
			_innerCache.Destroy();
			_outerCache.Destroy();
		}

		public override object Lock(object key)
		{
			var @lock = _innerCache.Lock(key);
			_actions.Enqueue(c => c.Lock(key));
			return @lock;
		}

		public override void Unlock(object key, object lockValue)
		{
			_innerCache.Unlock(key, lockValue);
			_actions.Enqueue(c => c.Unlock(key, lockValue));
		}

		public override  long NextTimestamp()
		{
			return _innerCache.NextTimestamp();
		}

		public override int Timeout
		{
			get { return _innerCache.Timeout; }
		}

		public override string RegionName
		{
			get { return _innerCache.RegionName; }
		}

		internal void Replay()
		{
			while (_actions.Count > 0)
			{
				var action = _actions.Dequeue();
				action.Invoke(_outerCache);
			}
		}
	}
}
