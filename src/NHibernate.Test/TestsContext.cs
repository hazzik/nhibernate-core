#if !NET461
using System.Configuration;
using System.IO;
using NHibernate.Cfg.ConfigurationSchema;
using NUnit.Framework;
using log4net.Repository.Hierarchy;
using NHibernate.Cfg;

namespace NHibernate.Test
{
	[SetUpFixture]
	public class TestsContext
	{
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			Environment.InitializeGlobalProperties(GetTestAssemblyHibernateConfiguration());

			ConfigureLog4Net();
		}

		public static IHibernateConfiguration GetTestAssemblyHibernateConfiguration()
		{
			string assemblyPath = Path.Combine(TestContext.CurrentContext.TestDirectory, Path.GetFileName(typeof(TestsContext).Assembly.Location));

			var configuration = ConfigurationManager.OpenExeConfiguration(assemblyPath);
			var section = configuration.GetSection(CfgXmlHelper.CfgSectionName);
			return HibernateConfiguration.FromAppConfig(section.SectionInformation.GetRawXml());
		}

		private static void ConfigureLog4Net()
		{
			var hierarchy = (Hierarchy)log4net.LogManager.GetRepository(typeof(TestsContext).Assembly);

			var consoleAppender = new log4net.Appender.ConsoleAppender()
			{
				Layout = new log4net.Layout.PatternLayout("%d{ABSOLUTE} %-5p %c{1}:%L - %m%n"),
			};

			((Logger)hierarchy.GetLogger("NHibernate.Hql.Ast.ANTLR")).Level = log4net.Core.Level.Off;
			((Logger)hierarchy.GetLogger("NHibernate.SQL")).Level = log4net.Core.Level.Off;
			((Logger)hierarchy.GetLogger("NHibernate.AdoNet.AbstractBatcher")).Level = log4net.Core.Level.Off;
			((Logger)hierarchy.GetLogger("NHibernate.Tool.hbm2ddl.SchemaExport")).Level = log4net.Core.Level.Error;
			hierarchy.Root.Level = log4net.Core.Level.Warn;
			hierarchy.Root.AddAppender(consoleAppender);
			hierarchy.Configured = true;
		}
	}
}
#endif
