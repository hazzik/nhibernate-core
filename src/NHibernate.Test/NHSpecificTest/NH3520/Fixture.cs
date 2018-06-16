using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3520
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void Should_retrieve_orderlines_when_getting_order()
		{
			using (var session = OpenSession())
			{
				using (var transaction = session.BeginTransaction())
				{
					var newOrder = Order.Create("Order products");
					var orderLine = OrderLine.Create("Product 1");
					newOrder.AddLine(orderLine);

					Assert.That(newOrder.Lines.Count, Is.EqualTo(1));
					session.Save(newOrder);
					session.Refresh(newOrder);
					Assert.That(newOrder.Lines.Count, Is.EqualTo(1));

					transaction.Commit();
				}

				using (var transaction = session.BeginTransaction())
				{
					var actual = session.Query<Order>().Single(x => x.Name == "Order products");
					actual.AddLine(OrderLine.Create("Product 2"));

					var actualLinesBeforeSave = actual.Lines;
					Assert.That(actualLinesBeforeSave.Count, Is.EqualTo(2));
					session.Save(actual);
					session.Refresh(actual);
					Assert.That(ReferenceEquals(actual.Lines, actualLinesBeforeSave));
					Assert.That(actual.Lines.Count, Is.EqualTo(2));

					transaction.Commit();
				}
			}
		}
	}
}
