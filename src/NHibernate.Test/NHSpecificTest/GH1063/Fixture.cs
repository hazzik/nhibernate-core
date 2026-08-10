using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1063
{
	// NH-1967: when a class is mapped more than once, under different entity-names, NHibernate
	// resolves the implicit entity-name for a bare instance (session.Save(new Person())) to
	// whichever mapping happens to be processed last, instead of preferring the mapping whose
	// entity-name is the class' own full name.
	[TestFixture]
	public class Fixture : TestCase
	{
		protected override string[] Mappings => new[] { "NHSpecificTest.GH1063.Mappings.hbm.xml" };

		protected override string MappingsAssembly => "NHibernate.Test";

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from PersonAlt").ExecuteUpdate();
				session.CreateQuery("delete from " + typeof(Person).FullName).ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void GuessedEntityNamePrefersTheMappingWithTheFullClassName()
		{
			var guessedEntityName = Sfi.TryGetGuessEntityName(typeof(Person));

			Assert.That(guessedEntityName, Is.EqualTo(typeof(Person).FullName));
		}

		[Test]
		public void SaveWithoutExplicitEntityNameUsesTheFullClassNameMapping()
		{
			using (var log = new SqlLogSpy())
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new Person { Name = "John" });
				transaction.Commit();

				Assert.That(log.GetWholeLog(), Does.Contain("insert into PersonDefault").IgnoreCase,
					"The row should have been inserted through the persister mapped with the class' full name.");
			}
		}
	}
}
