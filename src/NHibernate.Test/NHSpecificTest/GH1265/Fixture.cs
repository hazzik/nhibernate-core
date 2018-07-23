using System.Linq;
using System.Reflection;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1265
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		private SchemaExport _schemaExport;

		protected override void CreateSchema()
		{
			var config = TestConfigurationHelper.GetDefaultConfiguration();
			var type = GetType();
			config.AddResource(type.Namespace + ".MappingsForSchema.hbm.xml", type.Assembly);
			_schemaExport = new SchemaExport(config);
			_schemaExport.Create(false, true);
		}

		protected override void DropSchema()
		{
			DropSchema(false, _schemaExport, Sfi);
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var p = new Person
				{
					Name = "Bob"
				};
				session.Save(p);
				session.Save(
					new PersonChange
					{
						Person = p
					});

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from PersonChange").ExecuteUpdate();
				session.CreateQuery("delete from Person").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void QueryOnPersonChange()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var personChanges = s.Query<PersonChange>().ToList();
				Assert.That(personChanges, Has.Count.EqualTo(1));
				Assert.That(personChanges, Has.One.Not.Null);
				Assert.That(personChanges, Has.One.Property("Person").Not.Null);
				t.Commit();
			}
		}
	}
}
