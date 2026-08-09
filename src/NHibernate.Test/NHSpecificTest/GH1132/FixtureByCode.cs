using System.IO;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1132
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Foo>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.SchemaAction(SchemaAction.None);
					rc.Bag(
						x => x.Bars,
						m => m.Access(Accessor.Field),
						r => r.ManyToMany());
				});

			mapper.Class<Bar>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.SchemaAction(SchemaAction.None);
				});

			mapper.Class<Baz>(
				rc =>
				{
					rc.Id(x => x.Id);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		// Foo and Bar have schema-action="none", so their tables are legitimately never created.
		// TestCase's default clean-up check queries every mapped entity, which would fail for them;
		// skip it since this test does not persist any data.
		protected override bool CheckDatabaseWasCleaned()
		{
			return true;
		}

		[Test]
		public void SchemaActionNoneIsRespectedForManyToManyJoinTable()
		{
			var export = new NHibernate.Tool.hbm2ddl.SchemaExport(cfg);
			var writer = new StringWriter();
			export.Create(writer, false);
			var script = writer.ToString();

			Assert.That(script, Does.Match("create ((column|row) )?table Baz"), "Baz should be created since its schema action is the default (All).");
			Assert.That(script, Does.Not.Contain("Foo"), "Foo has schema-action=\"none\" and must not be created.");
			Assert.That(script, Does.Not.Contain("Bar"), "Bar has schema-action=\"none\" and must not be created.");
		}
	}
}
