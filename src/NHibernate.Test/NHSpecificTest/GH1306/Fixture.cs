using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1306
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var parent = new Parent {Id = 1L};
				s.Save(parent);
				s.Save(new Child {Id = 1L, ParentId = 1L, Status = 1});
				s.Save(new Child {Id = 2L, ParentId = 1L, Status = 0});
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Delete("from Child");
				s.Delete("from Parent");
				t.Commit();
			}
		}

		[Test]
		public void CanGetExtraLazyCollectionSizeWhenDynamicFilterIsEnabled()
		{
			using (var s = OpenSession())
			{
				s.EnableFilter("statusFilter").SetParameter("status", 1);

				var parent = s.Load<Parent>(1L);

				// Accessing Count on an extra-lazy collection triggers a "select count(...)"
				// query that must incorporate the enabled filter's parameter value.
				Assert.That(parent.Children.Count, Is.EqualTo(1));
			}
		}
	}
}
