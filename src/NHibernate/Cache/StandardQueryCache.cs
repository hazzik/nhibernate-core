using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Cache
{
	/// <summary>
	/// The standard implementation of the Hibernate <see cref="IQueryCache" />
	/// interface.  This implementation is very good at recognizing stale query
	/// results and re-running queries when it detects this condition, recaching
	/// the new results.
	/// </summary>
	public partial class StandardQueryCache : IQueryCache
	{
		private static readonly INHibernateLogger Log = NHibernateLogger.For(typeof (StandardQueryCache));
		private readonly UpdateTimestampsCache _updateTimestampsCache;

		/// <summary>
		/// Build a query cache.
		/// </summary>
		/// <param name="updateTimestampsCache">The cache of updates timestamps.</param>
		/// <param name="regionCache">The <see cref="CacheBase" /> to use for the region.</param>
		public StandardQueryCache(
			UpdateTimestampsCache updateTimestampsCache,
			CacheBase regionCache)
		{
			if (regionCache == null)
				throw new ArgumentNullException(nameof(regionCache));

			RegionName = regionCache.RegionName;
			Log.Info("starting query cache at region: {0}", RegionName);

			Cache = regionCache;
			_updateTimestampsCache = updateTimestampsCache;
		}

		#region IQueryCache Members

		public CacheBase Cache { get; }

		public string RegionName { get; }

		public void Clear()
		{
			Cache.Clear();
		}

		/// <inheritdoc />
		public bool Put(
			QueryKey key,
			QueryParameters queryParameters,
			ICacheAssembler[] returnTypes,
			IList result,
			ISessionImplementor session)
		{
			if (queryParameters.NaturalKeyLookup && result.Count == 0)
				return false;

			var ts = session.Factory.Settings.CacheProvider.NextTimestamp();

			Log.Debug("caching query results in region: '{0}'; {1}", RegionName, key);

			Cache.Put(key, GetCacheableResult(returnTypes, session, result, ts));

			return true;
		}

		/// <inheritdoc />
		public IList Get(
			QueryKey key,
			QueryParameters queryParameters,
			ICacheAssembler[] returnTypes,
			ISet<string> spaces,
			ISessionImplementor session)
		{
			var persistenceContext = session.PersistenceContext;
			var defaultReadOnlyOrig = persistenceContext.DefaultReadOnly;

			if (queryParameters.IsReadOnlyInitialized)
				persistenceContext.DefaultReadOnly = queryParameters.ReadOnly;
			else
				queryParameters.ReadOnly = persistenceContext.DefaultReadOnly;

			try
			{
				if (Log.IsDebugEnabled())
					Log.Debug("checking cached query results in region: '{0}'; {1}", RegionName, key);

				var cacheable = (IList) Cache.Get(key);
				if (cacheable == null)
				{
					Log.Debug("query results were not found in cache: {0}", key);
					return null;
				}

				var timestamp = (long) cacheable[0];

				if (Log.IsDebugEnabled())
					Log.Debug("Checking query spaces for up-to-dateness [{0}]", StringHelper.CollectionToString(spaces));

				if (!queryParameters.NaturalKeyLookup && !IsUpToDate(spaces, timestamp))
				{
					Log.Debug("cached query results were not up to date for: {0}", key);
					return null;
				}

				return GetResultFromCacheable(key, returnTypes, queryParameters.NaturalKeyLookup, session, cacheable);
			}
			finally
			{
				persistenceContext.DefaultReadOnly = defaultReadOnlyOrig;
			}
		}

		/// <inheritdoc />
		public bool[] PutMany(
			QueryKey[] keys,
			QueryParameters[] queryParameters,
			ICacheAssembler[][] returnTypes,
			IList[] results,
			ISessionImplementor session)
		{
			if (Log.IsDebugEnabled())
				Log.Debug("caching query results in region: '{0}'; {1}", RegionName, StringHelper.CollectionToString(keys));

			var cached = new bool[keys.Length];
			var ts = session.Factory.Settings.CacheProvider.NextTimestamp();
			var cachedKeys = new List<object>();
			var cachedResults = new List<object>();
			for (var i = 0; i < keys.Length; i++)
			{
				var result = results[i];
				if (queryParameters[i].NaturalKeyLookup && result.Count == 0)
					continue;

				cached[i] = true;
				cachedKeys.Add(keys[i]);
				cachedResults.Add(GetCacheableResult(returnTypes[i], session, result, ts));
			}

			Cache.PutMany(cachedKeys.ToArray(), cachedResults.ToArray());

			return cached;
		}

		/// <inheritdoc />
		public IList[] GetMany(
			QueryKey[] keys,
			QueryParameters[] queryParameters,
			ICacheAssembler[][] returnTypes,
			ISet<string>[] spaces,
			ISessionImplementor session)
		{
			if (Log.IsDebugEnabled())
				Log.Debug("checking cached query results in region: '{0}'; {1}", RegionName, StringHelper.CollectionToString(keys));

			var cacheables = Cache.GetMany(keys);

			var spacesToCheck = new List<ISet<string>>();
			var checkedSpacesIndexes = new HashSet<int>();
			var checkedSpacesTimestamp = new List<long>();
			for (var i = 0; i < keys.Length; i++)
			{
				var cacheable = (IList) cacheables[i];
				if (cacheable == null)
				{
					Log.Debug("query results were not found in cache: {0}", keys[i]);
					continue;
				}

				var querySpaces = spaces[i];
				if (queryParameters[i].NaturalKeyLookup || querySpaces.Count == 0)
					continue;

				spacesToCheck.Add(querySpaces);
				checkedSpacesIndexes.Add(i);
				// The timestamp is the first element of the cache result.
				checkedSpacesTimestamp.Add((long) cacheable[0]);
				if (Log.IsDebugEnabled())
					Log.Debug("Checking query spaces for up-to-dateness [{0}]", StringHelper.CollectionToString(querySpaces));
			}

			var upToDates = spacesToCheck.Count > 0
				? _updateTimestampsCache.AreUpToDate(spacesToCheck.ToArray(), checkedSpacesTimestamp.ToArray())
				: Array.Empty<bool>();

			var upToDatesIndex = 0;
			var persistenceContext = session.PersistenceContext;
			var defaultReadOnlyOrig = persistenceContext.DefaultReadOnly;
			var results = new IList[keys.Length];
			var finalReturnTypes = new ICacheAssembler[keys.Length][];
			try
			{
				session.PersistenceContext.BatchFetchQueue.InitializeQueryCacheQueue();

				for (var i = 0; i < keys.Length; i++)
				{
					var cacheable = (IList) cacheables[i];
					if (cacheable == null)
						continue;

					var key = keys[i];
					if (checkedSpacesIndexes.Contains(i) && !upToDates[upToDatesIndex++])
					{
						Log.Debug("cached query results were not up to date for: {0}", key);
						continue;
					}

					var queryParams = queryParameters[i];
					if (queryParams.IsReadOnlyInitialized)
						persistenceContext.DefaultReadOnly = queryParams.ReadOnly;
					else
						queryParams.ReadOnly = persistenceContext.DefaultReadOnly;

					Log.Debug("returning cached query results for: {0}", key);

					finalReturnTypes[i] = GetReturnTypes(key, returnTypes[i], cacheable);
					PerformBeforeAssemble(finalReturnTypes[i], session, cacheable);
				}

				for (var i = 0; i < keys.Length; i++)
				{
					if (finalReturnTypes[i] == null)
					{
						continue;
					}

					var queryParams = queryParameters[i];
					// Adjust the session cache mode, as PerformAssemble assemble types which may cause
					// entity loads, which may interact with the cache.
					using (session.SwitchCacheMode(queryParams.CacheMode))
					{
						try
						{
							results[i] = PerformAssemble(keys[i], finalReturnTypes[i], queryParams.NaturalKeyLookup, session, (IList) cacheables[i]);
						}
						finally
						{
							persistenceContext.DefaultReadOnly = defaultReadOnlyOrig;
						}
					}
				}

				for (var i = 0; i < keys.Length; i++)
				{
					if (finalReturnTypes[i] == null)
					{
						continue;
					}

					var queryParams = queryParameters[i];
					// Adjust the session cache mode, as InitializeCollections will initialize collections,
					// which may interact with the cache.
					using (session.SwitchCacheMode(queryParams.CacheMode))
					{
						try
						{
							InitializeCollections(finalReturnTypes[i], session, results[i], (IList) cacheables[i]);
						}
						finally
						{
							persistenceContext.DefaultReadOnly = defaultReadOnlyOrig;
						}
					}
				}
			}
			finally
			{
				session.PersistenceContext.BatchFetchQueue.TerminateQueryCacheQueue();
			}

			return results;
		}

		public void Destroy()
		{
			// The cache is externally provided and may be shared. Destroying the cache is
			// not the responsibility of this class.
		}

		#endregion

		private static List<object> GetCacheableResult(
			ICacheAssembler[] returnTypes,
			ISessionImplementor session,
			IList result,
			long ts)
		{
			var cacheable = new List<object>(result.Count + 1) { ts };
			foreach (var row in result)
			{
				if (returnTypes.Length == 1)
				{
					cacheable.Add(returnTypes[0].Disassemble(row, session, null));
				}
				else
				{
					cacheable.Add(TypeHelper.Disassemble((object[])row, returnTypes, null, session, null));
				}
			}

			return cacheable;
		}

		private static ICacheAssembler[] GetReturnTypes(
			QueryKey key,
			ICacheAssembler[] returnTypes,
			IList cacheable)
		{
			if (key.ResultTransformer?.AutoDiscoverTypes == true && cacheable.Count > 0)
			{
				returnTypes = GuessTypes(cacheable);
			}

			return returnTypes;
		}

		private static void PerformBeforeAssemble(
			ICacheAssembler[] returnTypes,
			ISessionImplementor session,
			IList cacheable)
		{
			if (returnTypes.Length == 1)
			{
				var returnType = returnTypes[0];

				// Skip first element, it is the timestamp
				for (var i = 1; i < cacheable.Count; i++)
				{
					returnType.BeforeAssemble(cacheable[i], session);
				}
			}
			else
			{
				// Skip first element, it is the timestamp
				for (var i = 1; i < cacheable.Count; i++)
				{
					TypeHelper.BeforeAssemble((object[]) cacheable[i], returnTypes, session);
				}
			}
		}

		private IList PerformAssemble(
			QueryKey key,
			ICacheAssembler[] returnTypes,
			bool isNaturalKeyLookup,
			ISessionImplementor session,
			IList cacheable)
		{
			try
			{
				var result = new List<object>(cacheable.Count - 1);
				if (returnTypes.Length == 1)
				{
					var returnType = returnTypes[0];

					// Skip first element, it is the timestamp
					for (var i = 1; i < cacheable.Count; i++)
					{
						result.Add(returnType.Assemble(cacheable[i], session, null));
					}
				}
				else
				{
					var nonCollectionTypeIndexes = new List<int>();
					for (var i = 0; i < returnTypes.Length; i++)
					{
						if (!(returnTypes[i] is CollectionType))
						{
							nonCollectionTypeIndexes.Add(i);
						}
					}

					// Skip first element, it is the timestamp
					for (var i = 1; i < cacheable.Count; i++)
					{
						result.Add(TypeHelper.Assemble((object[]) cacheable[i], returnTypes, nonCollectionTypeIndexes, session));
					}
				}

				return result;
			}
			catch (UnresolvableObjectException ex)
			{
				if (isNaturalKeyLookup)
				{
					//TODO: not really completely correct, since
					//      the UnresolvableObjectException could occur while resolving
					//      associations, leaving the PC in an inconsistent state
					Log.Debug(ex, "could not reassemble cached result set");
					// Handling a RemoveMany here does not look worth it, as this case short-circuits
					// the result-set. So a Many could only benefit batched queries, and only if many
					// of them are natural key lookup with an unresolvable object case.
					Cache.Remove(key);
					return null;
				}

				throw;
			}
		}

		private static void InitializeCollections(
			ICacheAssembler[] returnTypes,
			ISessionImplementor session,
			IList assembleResult,
			IList cacheResult)
		{
			var collectionIndexes = new Dictionary<int, ICollectionPersister>();
			for (var i = 0; i < returnTypes.Length; i++)
			{
				if (returnTypes[i] is CollectionType collectionType)
				{
					collectionIndexes.Add(i, session.Factory.GetCollectionPersister(collectionType.Role));
				}
			}

			if (collectionIndexes.Count == 0)
			{
				return;
			}

			// Skip first element, it is the timestamp
			for (var i = 1; i < cacheResult.Count; i++)
			{
				// Initialization of the fetched collection must be done at the end in order to be able to batch fetch them
				// from the cache or database. The collections were already created when their owners were assembled so we only
				// have to initialize them.
				TypeHelper.InitializeCollections(
					(object[]) cacheResult[i],
					(object[]) assembleResult[i - 1],
					collectionIndexes,
					session);
			}
		}

		private IList GetResultFromCacheable(
			QueryKey key,
			ICacheAssembler[] returnTypes,
			bool isNaturalKeyLookup,
			ISessionImplementor session,
			IList cacheable)
		{
			Log.Debug("returning cached query results for: {0}", key);
			returnTypes = GetReturnTypes(key, returnTypes, cacheable);
			try
			{
				session.PersistenceContext.BatchFetchQueue.InitializeQueryCacheQueue();

				PerformBeforeAssemble(returnTypes, session, cacheable);
				var result = PerformAssemble(key, returnTypes, isNaturalKeyLookup, session, cacheable);
				InitializeCollections(returnTypes, session, result, cacheable);
				return result;
			}
			finally
			{
				session.PersistenceContext.BatchFetchQueue.TerminateQueryCacheQueue();
			}
		}

		private static ICacheAssembler[] GuessTypes(IList cacheable)
		{
			var colCount = (cacheable[0] as object[])?.Length ?? 1;
			var returnTypes = new ICacheAssembler[colCount];
			if (colCount == 1)
			{
				foreach (var obj in cacheable)
				{
					if (obj == null)
						continue;
					returnTypes[0] = NHibernateUtil.GuessType(obj);
					break;
				}
			}
			else
			{
				var foundTypes = 0;
				foreach (object[] row in cacheable)
				{
					for (var i = 0; i < colCount; i++)
					{
						if (row[i] != null && returnTypes[i] == null)
						{
							returnTypes[i] = NHibernateUtil.GuessType(row[i]);
							foundTypes++;
						}
					}
					if (foundTypes == colCount)
						break;
				}
			}
			// If a column value was null for all rows, its type is still null: put a type which will just yield null
			// on null value.
			for (var i = 0; i < colCount; i++)
			{
				if (returnTypes[i] == null)
					returnTypes[i] = NHibernateUtil.String;
			}
			return returnTypes;
		}

		protected virtual bool IsUpToDate(ISet<string> spaces, long timestamp)
		{
			if (spaces.Count == 0)
				return true;

			return _updateTimestampsCache.IsUpToDate(spaces, timestamp);
		}
	}
}
