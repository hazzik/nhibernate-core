using System.Collections.Generic;
using NHibernate.Cache;

namespace NHibernate.Test.CacheTest.Caches
{
	public class BatchableCacheProvider : ICacheProvider
	{
		public CacheBase BuildCache(string regionName, IDictionary<string, string> properties)
		{
			return new BatchableCache(regionName);
		}

		public long NextTimestamp()
		{
			return Timestamper.Next();
		}

		public void Start(IDictionary<string, string> properties)
		{
		}

		public void Stop()
		{
		}
	}
}
