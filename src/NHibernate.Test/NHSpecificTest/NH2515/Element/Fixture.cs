using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2515.Element
{
	[TestFixture]
	public class Fixture : FixtureBase
	{
		private Guid _id;

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var parent = new Parent
					{
						Children =
							{
								"Child 1",
								"Child 2"
							},
					};
				_id = (Guid) session.Save(parent);
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object");

				transaction.Commit();
			}
		}

		[Test]
		public void ShouldDeleteChildren()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			using (var spy = new SqlLogSpy())
			{
				var parent = session.Get<Parent>(_id);

				session.Delete(parent);

				transaction.Commit();

				var length = CountDeletes(spy);

				Assert.AreEqual(1, length);
			}
		}
	}
}
