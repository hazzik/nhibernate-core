using NHibernate.Bytecode;
using NHibernate.Hql;
using NHibernate.Linq;

namespace NHibernate.Cfg.Loquacious
{
	public class FluentSessionFactoryConfiguration 
	{
		private readonly Configuration configuration;

		public FluentSessionFactoryConfiguration(Configuration configuration)
		{
			this.configuration = configuration;
			Integrate = new DbIntegrationConfiguration(configuration);
			Caching = new CacheConfiguration(this);
			Proxy = new ProxyConfiguration(this);
			GeneratingCollections = new CollectionFactoryConfiguration(this);
			Mapping = new MappingsConfiguration(this);
		}

		internal Configuration Configuration
		{
			get { return configuration; }
		}

		/// <summary>
		/// Set the SessionFactory mnemonic name.
		/// </summary>
		/// <param name="sessionFactoryName">The mnemonic name.</param>
		/// <returns>The fluent configuration itself.</returns>
		/// <remarks>
		/// The SessionFactory mnemonic name can be used as a surrogate key in a multi-DB application. 
		/// </remarks>
		public FluentSessionFactoryConfiguration Named(string sessionFactoryName)
		{
			configuration.SetProperty(Environment.SessionFactoryName, sessionFactoryName);
			return this;
		}

		/// <summary>
		/// DataBase integration configuration.
		/// </summary>
		public DbIntegrationConfiguration Integrate { get; }

		/// <summary>
		/// Cache configuration.
		/// </summary>
		public CacheConfiguration Caching { get; }

		public FluentSessionFactoryConfiguration GenerateStatistics()
		{
			configuration.SetProperty(Environment.GenerateStatistics, "true");
			return this;
		}

		public FluentSessionFactoryConfiguration DefaultFlushMode(FlushMode flushMode)
		{
			configuration.SetProperty(Environment.DefaultFlushMode, flushMode.ToString());
			return this;
		}

		public FluentSessionFactoryConfiguration ParsingHqlThrough<TQueryTranslator>()
			where TQueryTranslator : IQueryTranslatorFactory
		{
			configuration.SetProperty(Environment.QueryTranslator, typeof (TQueryTranslator).AssemblyQualifiedName);
			return this;
		}

		public FluentSessionFactoryConfiguration ParsingLinqThrough<TQueryProvider>()
			where TQueryProvider : INhQueryProvider
		{
			configuration.SetProperty(Environment.QueryLinqProvider, typeof(TQueryProvider).AssemblyQualifiedName);
			return this;
		}

		public ProxyConfiguration Proxy { get; }
		public CollectionFactoryConfiguration GeneratingCollections { get; }
		public MappingsConfiguration Mapping { get; }
	}

	public class CollectionFactoryConfiguration 
	{
		private readonly FluentSessionFactoryConfiguration fc;

		public CollectionFactoryConfiguration(FluentSessionFactoryConfiguration parent)
		{
			fc = parent;
		}

		public FluentSessionFactoryConfiguration Through<TCollectionsFactory>()
			where TCollectionsFactory : ICollectionTypeFactory
		{
			fc.Configuration.SetProperty(Environment.CollectionTypeFactoryClass,
										typeof (TCollectionsFactory).AssemblyQualifiedName);
			return fc;
		}
	}
}
