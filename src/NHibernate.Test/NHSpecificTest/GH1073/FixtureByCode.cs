using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1073
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Person>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Name);
			});

			mapper.JoinedSubclass<Employee>(rc =>
			{
				rc.Property(x => x.Title);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Person { Name = "wally" });
			session.Save(new Employee { Name = "dilbert", Title = "office clown" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from System.Object").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void QueryByClassInClauseReturnsAllMatchingSubtypes()
		{
			using var session = OpenSession();

			var count = session.CreateQuery("select p from Person p where p.class in (:classes)")
				.SetParameterList("classes", new[] { typeof(Person), typeof(Employee) })
				.List().Count;

			Assert.That(count, Is.EqualTo(2));
		}
	}
}
