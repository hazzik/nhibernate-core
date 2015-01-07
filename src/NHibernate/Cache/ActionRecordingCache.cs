using System;
using System.Collections.Generic;

namespace NHibernate.Cache
{
	internal class ActionRecordingCache : ICache
	{
		private readonly ICache _innerCache;
		private readonly ICache _outerCache;
		private readonly Queue<Action<ICache>> _actions;

		public ActionRecordingCache(string regionName, ICache outerCache)
			: this(new HashtableCache(regionName), outerCache)
		{
		}

		public ActionRecordingCache(ICache innerCache, ICache outerCache)
		{
			_innerCache = innerCache;
			_outerCache = outerCache;
			_actions = new Queue<Action<ICache>>();
		}

		public object Get(object key)
		{
			var instance = _innerCache.Get(key);
			if (instance != null)
			{
				return instance;
			}
			return _outerCache.Get(key);
		}

		public void Put(object key, object value)
		{
			_innerCache.Put(key, value);
			_actions.Enqueue(c => c.Put(key, value));
		}

		public void Remove(object key)
		{
			_innerCache.Remove(key);
			_actions.Enqueue(c => c.Remove(key));
		}

		public void Clear()
		{
			_innerCache.Clear();
			_actions.Clear();
			_actions.Enqueue(c => c.Clear());
		}

		public void Destroy()
		{
			_innerCache.Destroy();
			_outerCache.Destroy();
		}

		public void Lock(object key)
		{
			_innerCache.Lock(key);
			_actions.Enqueue(c => c.Lock(key));
		}

		public void Unlock(object key)
		{
			_innerCache.Unlock(key);
			_actions.Enqueue(c => c.Unlock(key));
		}

		public long NextTimestamp()
		{
			return _innerCache.NextTimestamp();
		}

		public int Timeout
		{
			get { return _innerCache.Timeout; }
		}

		public string RegionName
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