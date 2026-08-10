using System.Text;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1018
{
	// NH-3128
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Child>(
				rc =>
				{
					rc.Table("CHILDREN");
					rc.Id(x => x.Id, m => m.Generator(Generators.Increment));
				});

			mapper.JoinedSubclass<Parent>(
				rc =>
				{
					rc.Table("PARENTS");
					rc.Key(k => k.Column("Id"));
					rc.OneToOne(
						x => x.Info,
						m =>
						{
							m.Constrained(true);
							m.Cascade(Mapping.ByCode.Cascade.All);
						});
				});

			mapper.Class<Info>(
				rc =>
				{
					rc.Table("INFO");
					rc.Id(x => x.Id, m => m.Generator(Generators.Foreign<Info>(x => x.Parent)));
					rc.OneToOne<Parent>(x => x.Parent, m => { });
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		[Test]
		public void JoinedSubclassOneToOneGeneratesForeignKeyToBothTargets()
		{
			var script = new StringBuilder();
			SchemaExport.Execute(s => script.AppendLine(s), false, false);
			var sql = script.ToString();

			Assert.That(
				sql,
				Does.Match(@"(?is)alter table\s+PARENTS\s+add constraint.*?foreign key\s*\(Id\)\s*references\s+CHILDREN"),
				"Missing the foreign key from PARENTS to CHILDREN (the joined-subclass key).");

			Assert.That(
				sql,
				Does.Match(@"(?is)alter table\s+PARENTS\s+add constraint.*?foreign key\s*\(Id\)\s*references\s+INFO"),
				"Missing the foreign key from PARENTS to INFO (the constrained one-to-one).");
		}
	}
}
