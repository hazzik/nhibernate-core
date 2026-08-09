using System;
using System.Linq;
using NUnit.Framework;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	public class JoinTests : LinqTestCase
	{
		[Test]
		public void OrderLinesWith2ImpliedJoinShouldProduce2JoinsInSql()
		{
			//NH-3003
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.Customer.CompanyName == "Vins et alcools Chevalier"
							 select l).ToList();

				Assert.AreEqual(10, lines.Count);
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(2));
			}
		}

		[Test]
		public void OrderLinesWith2ImpliedJoinByIdShouldNotContainImpliedJoin()
		{
			//NH-2946 + NH-3003 = NH-2451
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.Customer.CustomerId == "VINET"
							 where l.Order.Customer.CompanyName == "Vins et alcools Chevalier"
							 select l).ToList();

				Assert.AreEqual(10, lines.Count);
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(2));
				Assert.That(Count(spy, "Orders"), Is.EqualTo(1));
			}
		}
		
		[Test]
		public void OrderLinesFilterByCustomerIdSelectLineShouldNotContainJoinWithCustomer()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.Customer.CustomerId == "VINET"
							 select l).ToList();

				Assert.AreEqual(10, lines.Count);
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
				Assert.That(Count(spy, "Customers"), Is.EqualTo(0));
			}
		}
		
		[Test]
		public void OrderLinesFilterByCustomerIdSelectCustomerIdShouldNotContainJoinWithCustomer()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.Customer.CustomerId == "VINET"
							 select l.Order.Customer.CustomerId).ToList();

				Assert.AreEqual(10, lines.Count);
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
				Assert.That(Count(spy, "Customers"), Is.EqualTo(0));
			}
		}
		
		[Test]
		public void OrderLinesFilterByCustomerIdSelectCustomerShouldContainJoinWithCustomer()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.Customer.CustomerId == "VINET"
							 select l.Order.Customer).ToList();

				Assert.AreEqual(10, lines.Count);
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(2));
				Assert.That(Count(spy, "Customers"), Is.EqualTo(1));
			}
		}
		
		[Test]
		public void OrderLinesFilterByCustomerCompanyNameAndSelectCustomerIdShouldJoinOrdersOnlyOnce()
		{
			//NH-2946 + NH-3003 = NH-2451
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.Customer.CompanyName == "Vins et alcools Chevalier"
							 select l.Order.Customer.CustomerId).ToList();

				Assert.AreEqual(10, lines.Count);
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(2));
				Assert.That(Count(spy, "Orders"), Is.EqualTo(1));
			}
		}
		
		[Test]
		public void OrderLinesFilterByOrderDateAndSelectOrderId()
		{
			//NH-2451
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.OrderDate < DateTime.Now
							 select l.Order.OrderId).ToList();

				Assert.AreEqual(2155, lines.Count);
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
			}
		}

		[Test]
		public void OrderLinesFilterByOrderIdAndSelectOrderDate()
		{
			//NH-2451
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.OrderId == 100
							 select l.Order.OrderDate).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
				Assert.That(Count(spy, "Orders"), Is.EqualTo(1));
			}
		}

		[Test]
		public void OrderLinesFilterByOrderIdAndSelectOrder()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				var lines = (from l in db.OrderLines
							 where l.Order.OrderId == 100
							 select l.Order).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
				Assert.That(Count(spy, "Orders"), Is.EqualTo(1));
			}
		}

		[Test]
		public void OrderLinesWithFilterByOrderIdShouldNotProduceJoins()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 where l.Order.OrderId == 1000
				 select l).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(0));
			}
		}
		
		[Test]
		public void OrderLinesWithFilterByOrderIdAndDateShouldProduceOneJoin()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 where l.Order.OrderId == 1000 && l.Order.OrderDate < DateTime.Now
				 select l).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
			}
		}

		[Test]
		public void OrderLinesWithOrderByOrderIdShouldNotProduceJoins()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 orderby l.Order.OrderId
				 select l).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(0));
			}
		}

		[Test]
		public void OrderLinesWithOrderByOrderShouldNotProduceJoins()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 orderby l.Order
				 select l).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(0));
			}
		}

		[Test]
		public void OrderLinesWithOrderByOrderIdAndDateShouldProduceOneJoin()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 orderby l.Order.OrderId, l.Order.OrderDate
				 select l).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
			}
		}

		[Test]
		public void OrderLinesWithSelectingOrderIdShouldNotProduceJoins()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 select l.Order.OrderId).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(0));
			}
		}

		[Test]
		public void OrderLinesWithSelectingOrderIdAndDateShouldProduceOneJoin()
		{
			//NH-2946
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 select new {l.Order.OrderId, l.Order.OrderDate}).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
			}
		}

		[Test(Description = "NH-3801")]
		public void OrderLinesWithSelectingCustomerIdInCaseShouldProduceOneJoin()
		{
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 select new { CustomerKnown = l.Order.Customer.CustomerId == null ? 0 : 1, l.Order.OrderDate }).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
			}
		}

		[Test(Description = "NH-3801"), Ignore("This is an ideal case, but not possible without better join detection")]
		public void OrderLinesWithSelectingCustomerInCaseShouldProduceOneJoin()
		{
			using (var spy = new SqlLogSpy())
			{
				// Without nominating the conditional to the select clause (and placing it in SQL)
				// [l.Order.Customer] will be selected in its entirety, creating a second join 
				(from l in db.OrderLines
				 select new { CustomerKnown = l.Order.Customer == null ? 0 : 1, l.Order.OrderDate }).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(1));
			}
		}

		[Test(Description = "NH-3801")]
		public void OrderLinesWithSelectingCustomerNameInCaseShouldProduceTwoJoins()
		{
			using (var spy = new SqlLogSpy())
			{
				(from l in db.OrderLines
				 select new { CustomerKnown = l.Order.Customer.CustomerId == null ? "unknown" : l.Order.Customer.CompanyName, l.Order.OrderDate }).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(2));
			}
		}

		[Test(Description = "NH-3801"), Ignore("This is an ideal case, but not possible without better join detection")]
		public void OrderLinesWithSelectingCustomerNameInCaseShouldProduceTwoJoinsAlternate()
		{
			using (var spy = new SqlLogSpy())
			{
				// Without nominating the conditional to the select clause (and placing it in SQL)
				// [l.Order.Customer] will be selected in its entirety, creating a second join 
				(from l in db.OrderLines
				 select new { CustomerKnown = l.Order.Customer == null ? "unknown" : l.Order.Customer.CompanyName, l.Order.OrderDate }).ToList();

				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(2));
			}
		}
		
		[Test]
		public void ShouldConstipateJoinsWhenOnlyComparingCompositeIdProperties()
		{
			using (var spy = new SqlLogSpy())
			{
				db.AnotherEntity.Where(x => x.CompositeIdEntity.Id.TenantId == 3).ToList();
				var countJoins = CountJoins(spy);
				Assert.That(countJoins, Is.EqualTo(0));
			}
		}

#if NET10_0_OR_GREATER
		[Test]
		public void LeftJoinShouldProduceLeftOuterJoin()
		{
			using (var spy = new SqlLogSpy())
			{
				db.Customers
					.LeftJoin(
						db.Orders,
						c => c.CustomerId,
						o => o.Customer.CustomerId,
						(c, o) => new { c.ContactName, OrderId = (int?) o.OrderId })
					.ToList();

				Assert.That(Count(spy, "left outer join"), Is.EqualTo(1));
				Assert.That(CountJoins(spy), Is.EqualTo(1));
			}
		}

		[Test]
		public void LeftJoinShouldYieldOuterElementsWithoutMatch()
		{
			var expected =
				(from c in db.Customers
				 join o in db.Orders on c.CustomerId equals o.Customer.CustomerId into orders
				 from o in orders.DefaultIfEmpty()
				 select new { c.CustomerId, OrderId = (int?) o.OrderId })
				.ToList();

			var actual = db.Customers
				.LeftJoin(
					db.Orders,
					c => c.CustomerId,
					o => o.Customer.CustomerId,
					(c, o) => new { c.CustomerId, OrderId = (int?) o.OrderId })
				.ToList();

			Assert.That(expected.Any(x => x.OrderId == null), Is.True, "Test data does not exercise unmatched rows");
			Assert.That(actual, Is.EquivalentTo(expected));
		}

		[Test]
		public void LeftJoinShouldYieldNullForUnmatchedInnerEntity()
		{
			var result = db.Customers
				.LeftJoin(db.Orders, c => c.CustomerId, o => o.Customer.CustomerId, (c, o) => new { c, o })
				.Where(x => x.o == null)
				.ToList();

			Assert.That(result, Is.Not.Empty);
			Assert.That(result.Select(x => x.c), Has.All.Not.Null);
		}

		[Test]
		public void LeftJoinShouldSupportCompositeKey()
		{
			using (var spy = new SqlLogSpy())
			{
				db.Customers
					.LeftJoin(
						db.Orders,
						c => new { c.CustomerId, HasContactTitle = c.ContactTitle != null },
						o => new { o.Customer.CustomerId, HasContactTitle = o.Customer.ContactTitle != null },
						(c, o) => new { c.ContactName, OrderId = (int?) o.OrderId })
					.ToList();

				// The explicit join, plus the one implied by the inner key selector referencing the
				// customer of the order.
				Assert.That(Count(spy, "left outer join"), Is.EqualTo(2));
			}
		}

		[Test]
		public void LeftJoinShouldSupportBeingChained()
		{
			using (var spy = new SqlLogSpy())
			{
				var result = db.Orders
					.LeftJoin(db.Customers, o => o.Customer.CustomerId, c => c.CustomerId, (o, c) => new { o, c })
					.LeftJoin(db.Employees, x => x.o.Employee.EmployeeId, e => e.EmployeeId, (x, e) => new { x.o.OrderId, x.c.ContactName, e.FirstName })
					.ToList();

				Assert.That(result, Is.Not.Empty);
				Assert.That(Count(spy, "left outer join"), Is.EqualTo(2));
			}
		}

		[Test]
		public void LeftJoinShouldSupportFilteredInnerSequence()
		{
			using (var spy = new SqlLogSpy())
			{
				db.Customers
					.LeftJoin(
						db.Orders.Where(o => o.Freight > 100),
						c => c.CustomerId,
						o => o.Customer.CustomerId,
						(c, o) => new { c.ContactName, OrderId = (int?) o.OrderId })
					.ToList();

				Assert.That(Count(spy, "left outer join"), Is.EqualTo(1));
			}
		}

		[Test]
		public void LeftJoinShouldSupportSubsequentOperators()
		{
			var result = db.Customers
				.LeftJoin(
					db.Orders,
					c => c.CustomerId,
					o => o.Customer.CustomerId,
					(c, o) => new { c.ContactName, OrderId = (int?) o.OrderId })
				.Where(x => x.OrderId == null)
				.OrderBy(x => x.ContactName)
				.Select(x => x.ContactName)
				.ToList();

			Assert.That(result, Is.Not.Empty);
			Assert.That(result, Is.Ordered);
		}
#endif

		private static int CountJoins(LogSpy sqlLog)
		{
			return Count(sqlLog, "join");
		}

		private static int Count(LogSpy sqlLog, string s)
		{
			var log = sqlLog.GetWholeLog();
			return log.Split(new[] {s}, StringSplitOptions.None).Length - 1;
		}
	}
}
