using System.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1265
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var p = new Person
				{
					Name = "Bob"
				};
				session.Save(p);
				session.Save(
					new PersonChange
					{
						Person = p
					});

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from PersonChange").ExecuteUpdate();
				session.CreateQuery("delete from Person").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void DoNothing()
		{
			
		}
	}
}
