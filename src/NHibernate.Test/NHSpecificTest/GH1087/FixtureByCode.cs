using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1087
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
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.OneToOne(x => x.Employee, m => m.Lazy(LazyRelation.Proxy));
				});

			mapper.Class<Employee>(
				rc => rc.Id(x => x.Id, m => m.Generator(Generators.Assigned)));

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Employee { Id = 1 });
			session.Save(new Person { Id = 1, Employee = session.Load<Employee>(1) });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Person").ExecuteUpdate();
			session.CreateQuery("delete from Employee").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void LazyProxyDoesNotEagerLoadOneToOneThroughPrimaryKey()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var person = session.Query<Person>().Single(x => x.Id == 1);

			Assert.That(NHibernateUtil.IsInitialized(person.Employee), Is.False);

			transaction.Commit();
		}
	}
}
