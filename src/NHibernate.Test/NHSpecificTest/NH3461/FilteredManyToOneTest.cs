using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3461
{
	public class FilteredManyToOneTest : BugTestCase
	{
		protected override void OnSetUp()
		{
			var parentA = new Parent("A");
			var parentB = new Parent("B");
			var childOfA = new Child("child of A", parentA);
			var childOfB = new Child("child of B", parentB);
			var childOfNoOne = new Child("child of no one", null);

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Persist(parentA);
				s.Persist(parentB);
				s.Persist(childOfA);
				s.Persist(childOfB);
				s.Persist(childOfNoOne);
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var t = session.BeginTransaction())
			{
				session.Delete("from Child");
				session.Delete("from Parent");
				t.Commit();
			}
		}


		[Test]
		public void UnfilteredQuery()
		{
			using (var session = OpenSession())
			{
				var threeChildren = session.QueryOver<Child>().List();
				Assert.That(threeChildren, Has.Count.EqualTo(3));
			}
		}

		[Test]
		public void FilteredQuery()
		{
			using (var session = OpenSession())
			{
				session.EnableFilter("NameFilter").SetParameter("AcceptedName", "A");
				var twoChildren = session.QueryOver<Child>().List();
				Assert.That(twoChildren, Has.Count.EqualTo(3));
			}
		}
	}
}
