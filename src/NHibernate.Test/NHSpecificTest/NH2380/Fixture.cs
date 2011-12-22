using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using SharpTestsEx;

namespace NHibernate.Test.NHSpecificTest.NH2380
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();

			using (var session = sessions.OpenStatelessSession())
			using (var tx = session.BeginTransaction())
			{
				foreach (var personName in new[] {"Foo", "Foo", "Bar", "Baz", "Soz", "Soz", "Tiz", "Fez"})
				{
					session.Insert(new Person
									   {
										   Name = personName,
										   Value = 100
									   });
				}
				tx.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = sessions.OpenStatelessSession())
			using (var tx = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object").ExecuteUpdate();
				tx.Commit();
			}
			base.OnTearDown();
		}

		protected override void Configure(Cfg.Configuration configuration)
		{
			configuration.SetProperty(Cfg.Environment.ShowSql, "true");
			base.Configure(configuration);
		}

		[Test]
		public void LinqProviderCanPerformDistinctOnAnonymousType()
		{
			using (var session = sessions.OpenSession())
			using (session.BeginTransaction())
			{
				var expected = session.CreateQuery("select distinct p.Name from Person p").List<string>();

				var actual = session.Query<Person>().Select(a => new {a.Name}).Distinct().ToArray();

				actual.Select(p => p.Name).Should().Have.SameValuesAs(expected);
			}
		}
		
		[Test]
		public void LinqProviderCanPerformDistinctOnComplexAnonymousType()
		{
			using (var session = sessions.OpenSession())
			using (session.BeginTransaction())
			{
				var expected = session.CreateQuery("select distinct p.Name from Person p").List<string>();

				var actual = session.Query<Person>().Select(a => new {a.Name, a.Value}).Distinct().ToArray();

				actual.Select(p => p.Name).Should().Have.SameValuesAs(expected);
			}
		}
		
		[Test]
		public void LinqProviderCanPerformDistinctOnTypeProjection()
		{
			using (var session = sessions.OpenSession())
			using (session.BeginTransaction())
			{
				var expected = session.CreateQuery("select distinct p.Name from Person p").List<string>();

				var actual = session.Query<Person>().Select(a => new PersonDto { Name = a.Name }).Distinct().ToArray();

				actual.Select(p => p.Name).Should().Have.SameValuesAs(expected);
			}
		}

		[Test]
		public void LinqProviderCanPerformDistinctOnTypeProjectionTwoProperty()
		{
			using (var session = sessions.OpenSession())
			using (session.BeginTransaction())
			{
				var expected = session.CreateQuery("select distinct p.Name from Person p").List<string>();

				var actual = session.Query<Person>().Select(a => new PersonDto { Name = a.Name, Value = a.Value}).Distinct().ToArray();

				actual.Select(p => p.Name).Should().Have.SameValuesAs(expected);
			}
		}

		[Test]
		public void LinqProviderCanPerformDistinctOnTypeProjectionWithCustomProjectionMethods()
		{
			using (var session = sessions.OpenSession())
			using (session.BeginTransaction())
			{
				var expected = session.CreateQuery("select distinct p.Name from Person p").List<string>();

				var actual = session.Query<Person>()
					.Select(a => new PersonDto
									 {
										 Name = Transform(a.Name),
										 Value = Transform(a.Value)
									 })
					.Distinct()
					.ToArray();

				actual.Select(p => p.Name).Should().Have.SameValuesAs(expected);
			}
		}

		public T Transform<T>(T value)
		{
			//just to enshure that covers NH-2645
			return value;
		}
	}
}
