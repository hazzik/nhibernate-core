using NUnit.Framework;
using NHibernate.Multi;

namespace NHibernate.Test.NHSpecificTest.NH1508
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override bool AppliesTo(Engine.ISessionFactoryImplementor factory)
		{
			return factory.ConnectionProvider.Driver.SupportsMultipleQueries;
		}

		protected override void OnSetUp()
		{
			var john = new Person();
			john.Name = "John";

			var doc1 = new Document();
			doc1.Person = john;
			doc1.Title = "John's Doc";

			var doc2 = new Document();
			doc2.Title = "Spec";
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Save(john);
				session.Save(doc1);
				session.Save(doc2);

				tx.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Delete("from Person");
				session.Delete("from Document");
				tx.Commit();
			}
		}

		[Test]
		public void DoesntThrowExceptionWhenHqlQueryIsGivenToQueryBatch()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var sqlQuery = session.CreateQuery("from Document");
				var q = session
				        .CreateQueryBatch()
				        .Add<Document>(sqlQuery);
				q.Execute();
			}
		}

		[Test]
		public void DoesntThrowsExceptionWhenSqlQueryIsGivenToQueryBatch()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var sqlQuery = session.CreateSQLQuery("select * from Document");
				var multiquery = session.CreateQueryBatch();
				Assert.That(() => multiquery.Add<object[]>(sqlQuery), Throws.Nothing);
				Assert.That(multiquery.Execute, Throws.Nothing);
			}
		}

		[Test]
		public void DoesntThrowsExceptionWhenNamedSqlQueryIsGivenToQueryBatch()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{

				var multiquery = session.CreateQueryBatch();
				var q = session.GetNamedQuery("SampleSqlQuery");
				Assert.That(() => multiquery.Add<int>(q), Throws.Nothing);
				Assert.That(multiquery.Execute, Throws.Nothing);
			}
		}
	}
}
