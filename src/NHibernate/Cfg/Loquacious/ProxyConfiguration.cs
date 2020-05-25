using NHibernate.Bytecode;

namespace NHibernate.Cfg.Loquacious
{
	public class ProxyConfiguration 
	{
		private readonly FluentSessionFactoryConfiguration fc;

		public ProxyConfiguration(FluentSessionFactoryConfiguration parent)
		{
			fc = parent;
		}

		public ProxyConfiguration DisableValidation()
		{
			fc.Configuration.SetProperty(Environment.UseProxyValidator, "false");
			return this;
		}

		public FluentSessionFactoryConfiguration Through<TProxyFactoryFactory>()
			where TProxyFactoryFactory : IProxyFactoryFactory
		{
			fc.Configuration.SetProperty(Environment.ProxyFactoryFactoryClass,
										typeof(TProxyFactoryFactory).AssemblyQualifiedName);
			return fc;
		}
	}

	public class ProxyConfigurationProperties
	{
		private readonly Configuration configuration;

		public ProxyConfigurationProperties(Configuration configuration)
		{
			this.configuration = configuration;
		}

		public bool Validation
		{
			set { configuration.SetProperty(Environment.UseProxyValidator, value.ToString().ToLowerInvariant()); }
		}

		public void ProxyFactoryFactory<TProxyFactoryFactory>() where TProxyFactoryFactory : IProxyFactoryFactory
		{
			configuration.SetProperty(Environment.ProxyFactoryFactoryClass,
			                          typeof(TProxyFactoryFactory).AssemblyQualifiedName);
		}
	}
}
