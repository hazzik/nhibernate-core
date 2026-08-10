using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg;
using NHibernate.Mapping.ByCode;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1048
{
	[TestFixture]
	public class Fixture
	{
		private Configuration _schemaToDrop;

		[OneTimeSetUp]
		public void CheckDialect()
		{
			var dialect = Dialect.Dialect.GetDialect(TestConfigurationHelper.GetDefaultConfiguration().Properties);
			if (!dialect.SupportsUnique)
				Assert.Ignore("This test requires a dialect supporting unique constraints");
		}

		[TearDown]
		public void TearDown()
		{
			if (_schemaToDrop != null)
				new SchemaExport(_schemaToDrop).Drop(false, true);
			_schemaToDrop = null;
		}

		[Test]
		public void SchemaUpdateReCreatesMissingUniqueConstraint()
		{
			// Create the table without a unique constraint on Code.
			var cfgWithoutUnique = GetConfiguration(unique: false);
			new SchemaExport(cfgWithoutUnique).Create(false, true);
			_schemaToDrop = cfgWithoutUnique;

			// Now update the schema against a mapping that requires Code to be unique.
			var cfgWithUnique = GetConfiguration(unique: true);
			var script = new List<string>();
			var schemaUpdate = new SchemaUpdate(cfgWithUnique);
			schemaUpdate.Execute(script.Add, true);

			Assert.That(schemaUpdate.Exceptions, Is.Empty, "SchemaUpdate reported exceptions");
			Assert.That(
				script.Any(s => s.ToLowerInvariant().Contains("unique") && s.ToLowerInvariant().Contains("code")),
				Is.True,
				"SchemaUpdate did not emit a statement to add the missing unique constraint on Widget.Code:" +
				System.Environment.NewLine + string.Join(System.Environment.NewLine, script));
		}

		private Configuration GetConfiguration(bool unique)
		{
			var cfg = TestConfigurationHelper.GetDefaultConfiguration();
			var mapper = new ModelMapper();
			mapper.Class<Widget>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Code, m => m.Unique(unique));
				});

			cfg.AddMapping(mapper.CompileMappingForAllExplicitlyAddedEntities());
			return cfg;
		}
	}
}
