using System.Collections.Generic;

namespace NHibernate.Cache
{
	/// <summary>
	/// Defines a factory for query cache instances.  These factories are responsible for
	/// creating individual QueryCache instances.
	/// </summary>
	public interface IQueryCacheFactory
	{
		/// <summary>
		/// Build a query cache.
		/// </summary>
		/// <param name="updateTimestampsCache">The cache of updates timestamps.</param>
		/// <param name="props">The NHibernate settings properties.</param>
		/// <param name="regionCache">The <see cref="CacheBase" /> to use for the region.</param>
		/// <returns>A query cache.</returns>
		IQueryCache GetQueryCache(
			UpdateTimestampsCache updateTimestampsCache,
			IDictionary<string, string> props,
			CacheBase regionCache);
	}
}
