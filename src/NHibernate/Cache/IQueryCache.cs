using System.Collections;
using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Cache
{
	/// <summary>
	/// Defines the contract for caches capable of storing query results.  These
	/// caches should only concern themselves with storing the matching result ids
	/// of entities.
	/// The transactional semantics are necessarily less strict than the semantics
	/// of an item cache.
	/// </summary>
	public partial interface IQueryCache
	{
		/// <summary>
		/// The underlying <see cref="CacheBase"/>.
		/// </summary>
		CacheBase Cache { get; }

		/// <summary>
		/// The cache region.
		/// </summary>
		string RegionName { get; }

		/// <summary>
		/// Clear the cache.
		/// </summary>
		void Clear();

		/// <summary>
		/// Clean up resources.
		/// </summary>
		/// <remarks>
		/// This method should not destroy <see cref="Cache" />. The session factory is responsible for it.
		/// </remarks>
		void Destroy();

		/// <summary>
		/// Get query results from the cache.
		/// </summary>
		/// <param name="key">The query key.</param>
		/// <param name="queryParameters">The query parameters.</param>
		/// <param name="returnTypes">The query result row types.</param>
		/// <param name="spaces">The query spaces.</param>
		/// <param name="session">The session for which the query is executed.</param>
		/// <returns>The query results, if cached.</returns>
		IList Get(
			QueryKey key, QueryParameters queryParameters, ICacheAssembler[] returnTypes, ISet<string> spaces,
			ISessionImplementor session);

		/// <summary>
		/// Put query results in the cache.
		/// </summary>
		/// <param name="key">The query key.</param>
		/// <param name="queryParameters">The query parameters.</param>
		/// <param name="returnTypes">The query result row types.</param>
		/// <param name="result">The query result.</param>
		/// <param name="session">The session for which the query was executed.</param>
		/// <returns><see langword="true" /> if the result has been cached, <see langword="false" />
		/// otherwise.</returns>
		bool Put(
			QueryKey key, QueryParameters queryParameters, ICacheAssembler[] returnTypes, IList result,
			ISessionImplementor session);

		/// <summary>
		/// Retrieve multiple query results from the cache.
		/// </summary>
		/// <param name="keys">The query keys.</param>
		/// <param name="queryParameters">The array of query parameters matching <paramref name="keys"/>.</param>
		/// <param name="returnTypes">The array of query result row types matching <paramref name="keys"/>.</param>
		/// <param name="spaces">The array of query spaces matching <paramref name="keys"/>.</param>
		/// <param name="session">The session for which the queries are executed.</param>
		/// <returns>The cached query results, matching each key of <paramref name="keys"/> respectively. For each
		/// missed key, it will contain a <see langword="null" />.</returns>
		IList[] GetMany(
			QueryKey[] keys, QueryParameters[] queryParameters, ICacheAssembler[][] returnTypes,
			ISet<string>[] spaces, ISessionImplementor session);

		/// <summary>
		/// Attempt to cache objects, after loading them from the database.
		/// </summary>
		/// <param name="keys">The query keys.</param>
		/// <param name="queryParameters">The array of query parameters matching <paramref name="keys"/>.</param>
		/// <param name="returnTypes">The array of query result row types matching <paramref name="keys"/>.</param>
		/// <param name="results">The array of query results matching <paramref name="keys"/>.</param>
		/// <param name="session">The session for which the queries were executed.</param>
		/// <returns>An array of boolean indicating if each query was successfully cached.</returns>
		/// <exception cref="CacheException"></exception>
		bool[] PutMany(
			QueryKey[] keys, QueryParameters[] queryParameters, ICacheAssembler[][] returnTypes, IList[] results,
			ISessionImplementor session);
	}
}
