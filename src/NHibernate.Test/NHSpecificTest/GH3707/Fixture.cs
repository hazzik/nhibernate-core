using System;
using System.Linq;
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3707
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is Oracle8iDialect;
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Entity
			{
				Name = "Bob", 
				DateTime1 = new DateTime(2015, 10, 21),
				DateTime2 = new DateTime(2015, 10, 21),
			});
			
			session.Save(new Entity
			{
				Name = "Alice", 
				DateTime1 = new DateTime(2015, 10, 21),
				DateTime2 = new DateTime(2015, 10, 21),
			});

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
		public void ShouldStoreSameValues()
		{
			using (var session = OpenSession())
			{
				using var transaction = session.BeginTransaction();

				session.CreateQuery("update Entity set DateTime1 = current_timestamp, DateTime2 = current_timestamp where name = 'Bob'")
				       .ExecuteUpdate();
				
				session.CreateQuery("update Entity set DateTime1 = localtimestamp, DateTime2 = localtimestamp where name = 'Alice'")
				       .ExecuteUpdate();

				transaction.Commit();
			}

			using (var session = OpenSession())
			{
				var bob = session
				             .Query<Entity>()
				             .Single(e => e.Name == "Bob");

				Assert.That(bob.DateTime1, Is.EqualTo(bob.DateTime2));
				Assert.That(bob.DateTime1.Kind, Is.EqualTo(DateTimeKind.Unspecified));
				Assert.That(bob.DateTime2.Kind, Is.EqualTo(DateTimeKind.Unspecified));
				Assert.That(bob.DateTime1, Is.EqualTo(DateTime.Now).Within(TimeSpan.FromSeconds(5)));
			}
		}
	}
}
