using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1960
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Person>(
				rc =>
				{
					rc.Id(x => x.Name, m => m.Generator(Generators.Assigned));
					rc.OneToOne(x => x.Employee, m => m.Lazy(LazyRelation.NoProxy));
				});

			mapper.Class<Employee>(
				rc =>
				{
					rc.Id(x => x.PersonName, m => m.Generator(Generators.Assigned));
					rc.OneToOne(x => x.Person, m => m.Lazy(LazyRelation.NoProxy));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var person = new Person { Name = "Gavin" };
			var employee = new Employee { PersonName = "Gavin", Person = person };
			person.Employee = employee;

			session.Save(person);
			session.Save(employee);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Employee").ExecuteUpdate();
			session.CreateQuery("delete from Person").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void LoadingOwnerDoesNotEagerlyLoadNonInverseNoProxyOneToOne()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var person = session.Get<Person>("Gavin");

			Assert.That(
				NHibernateUtil.IsPropertyInitialized(person, "Employee"),
				Is.False,
				"The lazy=\"no-proxy\" Employee association should not be initialized by merely loading its owning Person");
		}
	}
}
