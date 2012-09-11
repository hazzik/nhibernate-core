using NUnit.Framework;
using System.Linq;

namespace NHibernate.Test.NHSpecificTest.NH2922
{
	[TestFixture]
	public class SampleTest : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var a = new Store { Id = 1, Name = "A" };
				var b = new Store { Id = 2, Name = "B" };
				var jack = new Employee { Id = 3, Name = "Jack", Store = a };
				a.Staff.Add(jack);

				session.Save(a);
				session.Save(b);
				session.Save(jack);

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

		[Test(Description = "ObjectDeletedException should be thrown when source parent is loaded first")]
		[ExpectedException(typeof (ObjectDeletedException), ExpectedMessage = "cascade", MatchType = MessageMatch.Contains)]
		public void ExceptionShouldBeThrownIfChildIsReparentedCase1()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var a = session.Get<Store>(1); // Order of loading affects outcome
				var b = session.Get<Store>(2);

				// Move employee to different store
				var jack = a.Staff.Single(x => x.Name == "Jack");

				a.Staff.Remove(jack);
				jack.Store = b;
				b.Staff.Add(jack);

				tx.Commit();
			}
		}

		[Test(Description = "ObjectDeletedException should be thrown when target parent is loaded first")]
		[ExpectedException(typeof (ObjectDeletedException), ExpectedMessage = "cascade", MatchType = MessageMatch.Contains)]
		public void ExceptionShouldBeThrownIfChildIsReparentedCase2()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				var b = session.Get<Store>(2);
				var a = session.Get<Store>(1); // Order of loading affects outcome

				// Move employee to different store
				var jack = a.Staff.Single(x => x.Name == "Jack");

				a.Staff.Remove(jack);
				jack.Store = b;
				b.Staff.Add(jack);

				tx.Commit();
			}

			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var b = session.Get<Employee>(3);
				// The object has been deleted from the database
				Assert.IsNull(b);
			}
		}
	}
}
