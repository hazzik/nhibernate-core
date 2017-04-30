using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3998
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			try
			{
				ExecuteStatement("select test.Id from(select 1 as Id, 1 as id) as test");
			}
			catch
			{
				return false;
			}

			return true;
		}

		protected override void CreateSchema()
		{
			// NHibernate default create schema does not handle names differing by case sensitivity.
			// Manually create it.
			ExecuteStatement("create table Entity (Id uniqueidentifier not null, Name nvarchar(255), NaMe nvarchar(255))");
		}

		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var e1 = new Entity { Name = "Bob", NaMe = "BoB" };
				session.Save(e1);

				var e2 = new Entity { Name = "Sally", NaMe = "SaLlY" };
				session.Save(e2);

				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public void YourTestName()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = (from e in session.Query<Entity>()
							  where e.Name == "Bob"
							  select e).ToList();

				Assert.AreEqual(1, result.Count);
				Assert.AreEqual("BoB", result[0].NaMe);
			}
		}
	}
}