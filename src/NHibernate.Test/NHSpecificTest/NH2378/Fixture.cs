using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2378
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Save(new TestEntity
				{
					Id = 1,
					Name = "Test Entity1",
					TestPerson = new Person {Id = 1, Name = "TestUser1"}
				});

				session.Save(new TestEntity
				{
					Id = 2,
					Name = "Test Entity2",
					TestPerson = new Person {Id = 2, Name = "TestUser2"}
				});

				session.Save(new TestEntity
				{
					Id = 3,
					Name = "Test Entity3"
				});

				tx.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				tx.Commit();
			}
		}

		[Test]
		public void ShortEntityCanBeQueryCorrectlyUsingLinqProviderWhereByProjection()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var m = session.Query<TestEntity>()
					.Select(o => new TestEntityDto
					{
						EntityId = o.Id,
						EntityName = o.Name,
						PersonId = (o.TestPerson != null)
							? o.TestPerson.Id
							: (short) 0,
						PersonName = (o.TestPerson != null)
							? o.TestPerson.Name
							: string.Empty
					})
					.Where(o => o.PersonId == 2)
					.ToList();


				Assert.That(m.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void ShortEntityCanBeQueryCorrectlyUsingLinqProvider()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var m = session.Query<TestEntity>()
					.Select(o => new TestEntityDto
					{
						EntityId = o.Id,
						EntityName = o.Name,
						PersonId = (o.TestPerson != null)
							? o.TestPerson.Id
							: (short) 0,
						PersonName = (o.TestPerson != null)
							? o.TestPerson.Name
							: string.Empty
					})
					.OrderBy(o => o.EntityId)
					.ToList();

				Assert.That(m, Has.Count.EqualTo(3));
				Assert.That(m[0].PersonName, Is.EqualTo("TestUser1"));
				Assert.That(m[1].PersonName, Is.EqualTo("TestUser2"));
				Assert.That(m[2].PersonName, Is.Empty);
			}
		}
	}
}