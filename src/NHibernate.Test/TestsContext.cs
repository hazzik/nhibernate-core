using NUnit.Framework;

#if NETCOREAPP2_0
using System.Configuration;
using System.IO;
using NHibernate.Cfg;
using NHibernate.Cfg.ConfigurationSchema;
#endif

namespace NHibernate.Test
{
#if NETCOREAPP2_0
	[SetUpFixture]
#endif
	public class TestsContext
	{
		public static bool ExecutingWithVsTest { get; } =
			System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name == "testhost";

		public static void AssumeSystemTypeIsSerializable() =>
			Assume.That(typeof(System.Type).IsSerializable, Is.True);

#if NETCOREAPP2_0
		[OneTimeSetUp]
		public void RunBeforeAnyTests()
		{
			//When .NET Core App 2.0 tests run from VS/VSTest the entry assembly is "testhost.dll"
			//so we need to explicitly load the configuration
			if (ExecutingWithVsTest)
			{
				Environment.InitializeGlobalProperties(GetTestAssemblyHibernateConfiguration());
			}
		}

		public static IHibernateConfiguration GetTestAssemblyHibernateConfiguration()
		{
			var assemblyPath = Path.Combine(
				TestContext.CurrentContext.TestDirectory,
				Path.GetFileName(typeof(TestsContext).Assembly.Location));
			var configuration = ConfigurationManager.OpenExeConfiguration(assemblyPath);
			var section = configuration.GetSection(CfgXmlHelper.CfgSectionName);
			return HibernateConfiguration.FromAppConfig(section.SectionInformation.GetRawXml());
		}
#endif
	}
}
