namespace NHibernate.Cfg.Loquacious
{
	public class MappingsConfiguration 
	{
		private readonly FluentSessionFactoryConfiguration fc;

		public MappingsConfiguration(FluentSessionFactoryConfiguration parent)
		{
			fc = parent;
		}

		public MappingsConfiguration UsingDefaultCatalog(string defaultCatalogName)
		{
			fc.Configuration.SetProperty(Environment.DefaultCatalog, defaultCatalogName);
			return this;
		}

		public FluentSessionFactoryConfiguration UsingDefaultSchema(string defaultSchemaName)
		{
			fc.Configuration.SetProperty(Environment.DefaultSchema, defaultSchemaName);
			return fc;
		}
	}

	public class MappingsConfigurationProperties
	{
		private readonly Configuration configuration;

		public MappingsConfigurationProperties(Configuration configuration)
		{
			this.configuration = configuration;
		}

		public string DefaultCatalog
		{
			set { configuration.SetProperty(Environment.DefaultCatalog, value); }
		}

		public string DefaultSchema
		{
			set { configuration.SetProperty(Environment.DefaultSchema, value); }
		}
	}
}
