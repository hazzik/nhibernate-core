using System.Collections.Generic;
using NHibernate.Cfg;

namespace NHibernate.Cache
{
	/// <summary>
	/// Factory class for creating an <see cref="ICacheConcurrencyStrategy"/>.
	/// </summary>
	public static class CacheFactory
	{
		private static readonly INHibernateLogger log = NHibernateLogger.For(typeof(CacheFactory));

		public const string ReadOnly = "read-only";
		public const string ReadWrite = "read-write";
		public const string NonstrictReadWrite = "nonstrict-read-write";

		/// <remarks>
		/// No providers implement transactional caching currently,
		/// it was ported from Hibernate just for the sake of completeness.
		/// </remarks>
		public const string Transactional = "transactional";

		/// <summary>
		/// Creates an <see cref="ICacheConcurrencyStrategy"/> from the parameters.
		/// </summary>
		/// <param name="usage">The name of the strategy that <see cref="ICacheProvider"/> should use for the class.</param>
		/// <param name="cache">The <see cref="CacheBase"/> used for this strategy.</param>
		/// <returns>An <see cref="ICacheConcurrencyStrategy"/> to use for this object in the <see cref="CacheBase"/>.</returns>
		public static ICacheConcurrencyStrategy CreateCache(string usage, CacheBase cache)
		{
			if (log.IsDebugEnabled())
				log.Debug("cache for: {0} usage strategy: {1}", cache.RegionName, usage);

			ICacheConcurrencyStrategy ccs;
			switch (usage)
			{
				case ReadOnly:
					ccs = new ReadOnlyCache();
					break;
				case ReadWrite:
					ccs = new ReadWriteCache();
					break;
				case NonstrictReadWrite:
					ccs = new NonstrictReadWriteCache();
					break;
				//case CacheFactory.Transactional:
				//	ccs = new TransactionalCache();
				//	break;
				default:
					throw new MappingException(
						"cache usage attribute should be read-write, read-only or nonstrict-read-write");
			}

			ccs.Cache = cache;

			return ccs;
		}

		internal static CacheBase BuildCacheBase(string name, Settings settings, IDictionary<string, string> properties)
		{
			try
			{
				return settings.CacheProvider.BuildCache(name, properties);
			}
			catch (CacheException e)
			{
				throw new HibernateException("Could not instantiate cache implementation", e);
			}
		}
	}
}
