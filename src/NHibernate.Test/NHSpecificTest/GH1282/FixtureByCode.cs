using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1282
{
	// Person and PersonDetails are unrelated root mappings (no discriminator, no shared
	// hierarchy) that both target the same table, even though PersonDetails happens to
	// derive from Person in .NET. Querying for Person therefore triggers NHibernate's
	// implicit polymorphism: the query is duplicated once per implementor (Person and
	// PersonDetails) and each duplicate independently applies Take/limit, before the
	// per-implementor results get concatenated.
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Person>(
				rc =>
				{
					rc.Table("GH1282Person");
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
				});

			mapper.Class<PersonDetails>(
				rc =>
				{
					rc.Table("GH1282Person");
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
					rc.Property(x => x.Address);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Person { Id = 1, Name = "John" });
			session.Save(new PersonDetails { Id = 2, Name = "Jane", Address = "Main street" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from PersonDetails").ExecuteUpdate();
			session.CreateQuery("delete from Person").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void TakeLimitsTotalRowCount()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var result = session.Query<Person>().Take(1).ToList();

			Assert.That(result, Has.Count.EqualTo(1));

			transaction.Commit();
		}
	}
}
