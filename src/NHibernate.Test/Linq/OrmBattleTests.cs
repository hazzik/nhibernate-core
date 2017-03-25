using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using NHibernate.DomainModel.Northwind.Entities;
using NHibernate.Exceptions;
using NHibernate.Hql.Ast.ANTLR;
using NUnit.Framework;
using Remotion.Linq.Parsing;

namespace NHibernate.Test.Linq
{
	[TestFixture]
	public class OrmBattleTests : LinqTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();

			Customers = db.Customers.ToList();
			Employees = db.Employees.ToList();
			Orders = db.Orders.ToList();
			Products = db.Products.ToList();
		}

		private List<Customer> Customers;
		private List<Employee> Employees;
		private List<Order> Orders;
		private List<Product> Products;

		// DTO for testing purposes.
		public class OrderDTO
		{
			public int Id { get; set; }
			public string CustomerId { get; set; }
			public DateTime? OrderDate { get; set; }
		}

		#region Filtering tests

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereTest()
		{
			var result = from o in db.Orders
						 where o.ShippingAddress.City == "Seattle"
						 select o;
			var expected = from o in Orders
						   where o.ShippingAddress.City == "Seattle"
						   select o;
			var list = result.ToList();
			Assert.AreEqual(14, list.Count);
			Assert.AreEqual(0, expected.Except(list).Count());
		}

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereParameterTest()
		{
			var city = "Seattle";
			var result = from o in db.Orders
						 where o.ShippingAddress.City == city
						 select o;
			var expected = from o in Orders
						   where o.ShippingAddress.City == city
						   select o;
			var list = result.ToList();
			Assert.AreEqual(14, list.Count);
			Assert.AreEqual(0, expected.Except(list).Count());

			city = "Rio de Janeiro";
			list = result.ToList();
			Assert.AreEqual(34, list.Count);
			Assert.AreEqual(0, expected.Except(list).Count());
		}

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereConditionsTest()
		{
			var result = from p in db.Products
						 where p.UnitsInStock < p.ReorderLevel && p.UnitsOnOrder == 0
						 select p;
			var list = result.ToList();
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		[Category("Filtering")]
		// Passed.
			//TODO: fix me!
		public void WhereNullTest()
		{
			var result = from o in db.Orders
						 where o.ShippingAddress.Region == null
						 select o;
			var list = result.ToList();
			Assert.AreEqual(0, list.Count); //should be 507
		}

		[Test]
		[Category("Filtering")]
		// Passed.
			//TODO: fix me.S
		public void WhereNullParameterTest()
		{
			string region = null;
			var result = from o in db.Orders
						 where o.ShippingAddress.Region == region
						 select o;
			var list = result.ToList();
			Assert.AreEqual(0, list.Count); //should be 507

			region = "WA";
			list = result.ToList();
			Assert.AreEqual(19, list.Count);
		}

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereNullableTest()
		{
			var result = from o in db.Orders
						 where !o.ShippingDate.HasValue
						 select o;
			var list = result.ToList();
			Assert.AreEqual(21, list.Count);
		}

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereNullableParameterTest()
		{
			DateTime? shippedDate = null;
			var result = from o in db.Orders
						 where o.ShippingDate == shippedDate
						 select o;
			var list = result.ToList();
			Assert.AreEqual(21, list.Count);
		}

		[Test]
		[Category("Filtering")]
		// Passed. 
		//TODO: fix me1
		public void WhereCoalesceTest()
		{
			var result = from o in db.Orders
						 where (o.ShippingAddress.Region ?? "N/A") == "N/A"
						 select o;
			var list = result.ToList();
			Assert.AreEqual(0, list.Count); //should be 507
		}

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereConditionalTest()
		{
			var result = from o in db.Orders
						 where (o.ShippingAddress.City == "Seattle" ? "Home" : "Other") == "Home"
						 select o;
			var list = result.ToList();
			Assert.AreEqual(14, list.Count);
		}

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereConditionalBooleanTest()
		{
			var result = from o in db.Orders
						 where o.ShippingAddress.City == "Seattle" ? true : false
						 select o;
			var list = result.ToList();
			Assert.AreEqual(14, list.Count);
		}

		[Test]
		[Category("Filtering")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   NewExpression
		public void WhereAnonymousParameterTest()
		{
			var cityRegion = new {City = "Seattle", Region = "WA"};
			var result = from o in db.Orders
						 where new {o.ShippingAddress.City, o.ShippingAddress.Region} == cityRegion
						 select o;
			var list = result.ToList();
			Assert.AreEqual(14, list.Count);
		}

		[Test]
		[Category("Filtering")]
		// Passed.
		public void WhereEntityParameterTest()
		{
			var order = db.Orders.OrderBy(o => o.OrderDate).First();
			var result = from o in db.Orders
						 where o == order
						 select o;
			var list = result.ToList();
			Assert.AreEqual(1, list.Count);
			Assert.AreEqual(order, list[0]);
			Assert.AreSame(order, list[0]);
		}

		#endregion

		#region Projection tests

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectTest()
		{
			var result = from o in db.Orders
						 select o.ShippingAddress.Region;
			var expected = from o in Orders
						   select o.ShippingAddress.Region;
			var list = result.ToList();
			Assert.AreEqual(expected.Count(), list.Count);
			Assert.AreEqual(0, expected.Except(list).Count());
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectBooleanTest()
		{
			var result = from o in db.Orders
						 select o.ShippingAddress.Region == "WA";
			var expected = from o in Orders
						   select o.ShippingAddress.Region == "WA";
			var list = result.ToList();
			Assert.AreEqual(expected.Count(), list.Count);
			Assert.AreEqual(0, expected.Except(list).Count());
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectCalculatedTest()
		{
			var result = from o in db.Orders
						 select o.Freight*1000;
			var expected = from o in Orders
						   select o.Freight*1000;
			var list = result.ToList();
			var expectedList = expected.ToList();
			list.Sort();
			expectedList.Sort();

			// Assert.AreEqual(expectedList.Count, list.Count);
			// expectedList.Zip(list, (i, j) => {
			//                       Assert.AreEqual(i,j);
			//                       return true;
			//                     });
			NUnit.Framework.CollectionAssert.AreEquivalent(expectedList, list);
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectNestedCalculatedTest()
		{
			var result = from r in
							 from o in db.Orders
							 select o.Freight*1000
						 where r > 100000
						 select r/1000;
			var expected = from o in Orders
						   where o.Freight > 100
						   select o.Freight;
			var list = result.ToList();
			var expectedList = expected.ToList();
			list.Sort();
			expectedList.Sort();
			Assert.AreEqual(187, list.Count);
			// Assert.AreEqual(expectedList.Count, list.Count);
			// expectedList.Zip(list, (i, j) => {
			//                       Assert.AreEqual(i,j);
			//                       return true;
			//                     });
			NUnit.Framework.CollectionAssert.AreEquivalent(expectedList, list);
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectAnonymousTest()
		{
			var result = from o in db.Orders
						 select new {OrderID = o.OrderId, o.OrderDate, o.Freight};
			var expected = from o in Orders
						   select new {OrderID = o.OrderId, o.OrderDate, o.Freight};
			var list = result.ToList();
			Assert.AreEqual(expected.Count(), list.Count);
			Assert.AreEqual(0, expected.Except(list).Count());
		}

		[Test]
		[Category("Projections")]
		//[ExpectedException(typeof(ArgumentException))]
		// Failed.
		// Inner Exception: ArgumentException
		// Message:
		//   The value "OrmBattle.NHibernateModel.Northwind.Customer" is not of type "System.Linq.IQueryable`1[OrmBattle.NHibernateModel.Northwind.Customer]" and cannot be used in this generic collection.
		//   Parameter name: value
		public void SelectSubqueryTest()
		{
			var result = from o in db.Orders
						 select db.Customers.Where(c => c.CustomerId == o.Customer.CustomerId);

			var expected = from o in Orders
						   select Customers.Where(c => c.CustomerId == o.Customer.CustomerId);
			var list = result.ToList();

			var expectedList = expected.ToList();
			NUnit.Framework.CollectionAssert.AreEquivalent(expectedList, list);

			//Assert.AreEqual(expected.Count(), list.Count);
			//expected.Zip(result, (expectedCustomers, actualCustomers) => {
			//                       Assert.AreEqual(expectedCustomers.Count(), actualCustomers.Count());
			//                       Assert.AreEqual(0, expectedCustomers.Except(actualCustomers));
			//                       return true;
			//                     });
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectDtoTest()
		{
			var result = from o in db.Orders
						 select new OrderDTO {Id = o.OrderId, CustomerId = o.Customer.CustomerId, OrderDate = o.OrderDate};
			var list = result.ToList();
			Assert.AreEqual(Orders.Count(), list.Count);
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectNestedDtoTest()
		{
			var result = from r in
							 from o in db.Orders
							 select new OrderDTO {Id = o.OrderId, CustomerId = o.Customer.CustomerId, OrderDate = o.OrderDate}
						 where r.OrderDate > new DateTime(1998, 01, 01)
						 select r;
			var list = result.ToList();
			Assert.AreEqual(267, list.Count);
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectManyAnonymousTest()
		{
			var result = from c in db.Customers
						 from o in c.Orders
						 where o.Freight < 500.00M
						 select new {c.CustomerId, o.OrderId, o.Freight};
			var list = result.ToList();
			Assert.AreEqual(817, list.Count);
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectManyLetTest()
		{
			var result = from c in db.Customers
						 from o in c.Orders
						 let freight = o.Freight
						 where freight < 500.00M
						 select new {c.CustomerId, o.OrderId, freight};
			var list = result.ToList();
			Assert.AreEqual(817, list.Count);
		}

		[Test]
		[Category("Projections")]
		//[ExpectedException(typeof(HibernateException))]
		// Failed.
		// Exception: HibernateException 
		// Message:
		//   Query Source could not be identified: ItemName = g, ItemType = System.Linq.IGrouping`2[NHibernate.DomainModel.Northwind.Entities.Customer,NHibernate.DomainModel.Northwind.Entities.Order], Expression = from IGrouping`2 g in {value(NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Order]) => GroupBy([o].Customer, [o])}
		public void SelectManyGroupByTest()
		{
			var result = db.Orders
				.GroupBy(o => o.Customer)
				.Where(g => g.Count() > 20)
				.SelectMany(g => g.Select(o => o.Customer));

			var list = result.ToList();
			Assert.AreEqual(89, list.Count);
		}

		[Test]
		[Category("Projections")]
		// Passed.
		public void SelectManyOuterProjectionTest()
		{
			var result = db.Customers.SelectMany(i => i.Orders.Select(t => i));

			var list = result.ToList();
			Assert.AreEqual(830, list.Count);
		}

		[Test]
		[Category("Projections")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   The DefaultIfEmptyResultOperator result operator is not current supported
		public void SelectManyLeftJoinTest()
		{
			var result =
				from c in db.Customers
				from o in c.Orders.Select(o => new {o.OrderId, c.CompanyName}).DefaultIfEmpty()
				select new {c.ContactName, o};

			var list = result.ToList();
			Assert.Greater(list.Count, 0);
		}

		#endregion

		#region Take / Skip tests

		[Test]
		[Category("Take/Skip")]
		// Passed.
		public void TakeTest()
		{
			var result = (from o in db.Orders
						  orderby o.OrderDate , o.OrderId
						  select o).Take(10);
			var expected = (from o in Orders
							orderby o.OrderDate , o.OrderId
							select o).Take(10);
			var list = result.ToList();
			Assert.AreEqual(10, list.Count);
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Take/Skip")]
		// Passed.
		public void SkipTest()
		{
			var result = (from o in db.Orders
						  orderby o.OrderDate , o.OrderId
						  select o).Skip(10);
			var expected = (from o in Orders
							orderby o.OrderDate , o.OrderId
							select o).Skip(10);
			var list = result.ToList();
			Assert.AreEqual(820, list.Count);
			Assert.IsTrue(expected.SequenceEqual(result));

		}

		[Test]
		[Category("Take/Skip")]
		// Passed.
		public void TakeSkipTest()
		{
			var result = (from o in db.Orders
						  orderby o.OrderDate , o.OrderId
						  select o).Skip(10).Take(10);
			var expected = (from o in Orders
							orderby o.OrderDate , o.OrderId
							select o).Skip(10).Take(10);
			var list = result.ToList();
			Assert.AreEqual(10, list.Count);
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Take/Skip")]
		//[ExpectedException(typeof(GenericADOException))]
		// Failed.
		// Inner Exception: SqlException
		// Message:
		//   Subquery returned more than 1 value. This is not permitted when the subquery follows =, !=, <, <= , >, >= or when the subquery is used as an expression.
		public void TakeNestedTest()
		{
			var result =
				from c in db.Customers
				select new {Customer = c, TopOrders = c.Orders.OrderByDescending(o => o.OrderDate).Take(5)};
			var expected =
				from c in Customers
				select new {Customer = c, TopOrders = c.Orders.OrderByDescending(o => o.OrderDate).Take(5)};
			var list = result.ToList();
			Assert.AreEqual(expected.Count(), list.Count);
			foreach (var anonymous in list)
			{
				var count = anonymous.TopOrders.ToList().Count;
				Assert.GreaterOrEqual(count, 0);
				Assert.LessOrEqual(count, 5);
			}
		}

		[Test]
		[Category("Take/Skip")]
		// Passed.
		public void ComplexTakeSkipTest()
		{
			var original = db.Orders.ToList()
				.OrderBy(o => o.OrderDate)
				.Skip(100)
				.Take(50)
				.OrderBy(o => o.RequiredDate)
				.Where(o => o.OrderDate != null)
				.Select(o => o.RequiredDate)
				.Distinct()
				.Skip(10);
			var result = db.Orders
				.OrderBy(o => o.OrderDate)
				.Skip(100)
				.Take(50)
				.OrderBy(o => o.RequiredDate)
				.Where(o => o.OrderDate != null)
				.Select(o => o.RequiredDate)
				.Distinct()
				.Skip(10);
			var originalList = original.ToList();
			var resultList = result.ToList();
			Assert.AreEqual(originalList.Count, resultList.Count);
			Assert.IsTrue(originalList.SequenceEqual(resultList));
		}

		#endregion

		#region Ordering tests

		[Test]
		[Category("Ordering")]
		// Passed.
		public void OrderByTest()
		{
			var result =
				from o in db.Orders
				orderby o.OrderDate , o.ShippingDate descending , o.OrderId
				select o;
			var expected =
				from o in Orders
				orderby o.OrderDate , o.ShippingDate descending , o.OrderId
				select o;

			var list = result.ToList();
			var expectedList = expected.ToList();
			Assert.AreEqual(expectedList.Count, list.Count);
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		// Passed.
		public void OrderByWhereTest()
		{
			var result = (from o in db.Orders
						  orderby o.OrderDate , o.OrderId
						  where o.OrderDate > new DateTime(1997, 1, 1)
						  select o).Take(10);
			var expected = (from o in Orders
							where o.OrderDate > new DateTime(1997, 1, 1)
							orderby o.OrderDate , o.OrderId
							select o).Take(10);
			var list = result.ToList();
			Assert.AreEqual(10, list.Count);
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		// Passed.
		public void OrderByCalculatedColumnTest()
		{
			var result =
				from o in db.Orders
				orderby o.Freight*o.OrderId descending
				select o;
			var expected =
				from o in Orders
				orderby o.Freight*o.OrderId descending
				select o;
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		// Passed.
		public void OrderByEntityTest()
		{
			var result =
				from o in db.Orders
				orderby o
				select o;
			var expected =
				from o in Orders
				orderby o.OrderId
				select o;
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   NewExpression
		public void OrderByAnonymousTest()
		{
			var result =
				from o in db.Orders
				orderby new {o.OrderDate, o.ShippingDate, o.OrderId}
				select o;
			var expected =
				from o in Orders
				orderby new { o.OrderDate, o.ShippingDate, o.OrderId }
				select o;
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		// Passed.
		public void OrderByMultipleTest()
		{
			var result =
				from o in db.Orders
				orderby o.OrderDate, o.ShippingDate, o.OrderId//new {o.OrderDate, o.ShippingDate, o.OrderId}
				select o;
			var expected =
				from o in Orders
				orderby o.OrderDate , o.ShippingDate , o.OrderId
				select o;
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   Specified method is not supported.
		public void OrderByDistinctTest()
		{
			var result = db.Customers
				.OrderBy(c => c.CompanyName)
				.Select(c => c.Address.City)
				.Distinct()
				.OrderBy(c => c)
				.Select(c => c);
			var expected = Customers
				.OrderBy(c => c.CompanyName)
				.Select(c => c.Address.City)
				.Distinct()
				.OrderBy(c => c)
				.Select(c => c);
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   Specified method is not supported.
		public void OrderBySelectManyTest()
		{
			var result =
				from c in db.Customers.OrderBy(c => c.ContactName)
				from o in db.Orders.OrderBy(o => o.OrderDate)
				where c == o.Customer
				select new {c.ContactName, o.OrderDate};
			var expected =
				from c in Customers.OrderBy(c => c.ContactName)
				from o in Orders.OrderBy(o => o.OrderDate)
				where c == o.Customer
				select new {c.ContactName, o.OrderDate};
			Assert.IsTrue(expected.SequenceEqual(result));
		}

		[Test]
		[Category("Ordering")]
		//[ExpectedException(typeof(QuerySyntaxException))]
		// Failed.
		// Exception: QuerySyntaxException
		// Message:
		//   Exception of type 'Antlr.Runtime.NoViableAltException' was thrown. [.Select[OrmBattle.NHibernateModel.Northwind.Order,System.Int32](.ThenBy[OrmBattle.NHibernateModel.Northwind.Order,System.Int32](.OrderBy[OrmBattle.NHibernateModel.Northwind.Order,System.Boolean](NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Order], Quote((o, ) => (AndAlso(Decimal.op_GreaterThan(o.Freight, p1), DateTime.op_Inequality(o.ShippedDate, NULLp2)))), ), Quote((o, ) => (o.Id)), ), Quote((o, ) => (o.Id)), )]
		public void OrderByPredicateTest()
		{
			var result = db.Orders.OrderBy(o => o.Freight > 0 && o.ShippingDate != null).ThenBy(o => o.OrderId).Select(o => o.OrderId);
			var list = result.ToList();
			var original = Orders.OrderBy(o => o.Freight > 0 && o.ShippingDate != null).ThenBy(o => o.OrderId).Select(o => o.OrderId).ToList();
			Assert.IsTrue(list.SequenceEqual(original));
		}

		#endregion

		#region Grouping tests

		[Test]
		[Category("Grouping")]
		// Passed.
		public void GroupByTest()
		{
			var result = from o in db.Orders
						 group o by o.OrderDate;
			var list = result.ToList();
			Assert.AreEqual(481, list.Count);
		}

		[Test]
		[Category("Grouping")]
		// Passed.
		public void GroupByReferenceTest()
		{
			var result = from o in db.Orders
						 group o by o.Customer;
			var list = result.ToList();
			Assert.AreEqual(89, list.Count);
		}

		[Test]
		[Category("Grouping")]
		//[ExpectedException(typeof(HibernateException))]
		// Failed with assertion.
		// Exception: HibernateException
		// Message:
		//     Query Source could not be identified: ItemName = g, ItemType = System.Linq.IGrouping`2[System.Nullable`1[System.DateTime],NHibernate.DomainModel.Northwind.Entities.Order], Expression = from IGrouping`2 g in {value(NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Order]) => GroupBy([o].OrderDate, [o])}
		public void GroupByWhereTest()
		{
			var result =
				from o in db.Orders
				group o by o.OrderDate
				into g
				where g.Count() > 5
				select g;
			var list = result.ToList();
			Assert.AreEqual(1, list.Count);
		}

		[Test]
		[Category("Grouping")]
		// Passed.
		public void GroupByTestAnonymous()
		{
			var result = from c in db.Customers
						 group c by new {c.Address.Region, c.Address.City};
			var list = result.ToList();
			Assert.AreEqual(69, list.Count);
		}

		[Test]
		[Category("Grouping")]
		// Passed.
		public void GroupByCalculatedTest()
		{
			var result =
				from o in db.Orders
				group o by o.Freight > 50 ? o.Freight > 100 ? "expensive" : "average" : "cheap"
				into g
				select g;
			var list = result.ToList();
			Assert.AreEqual(3, list.Count);
		}

		[Test]
		[Category("Grouping")]
		//[ExpectedException(typeof(HibernateException))]
		// Failed.
		// Exception: HibernateException 
		// Message:
		//   Query Source could not be identified: ItemName = g, ItemType = System.Linq.IGrouping`2[System.String,NHibernate.DomainModel.Northwind.Entities.Customer], Expression = from IGrouping`2 g in {value(NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Customer]) => GroupBy([c].Address.City, [c])}
		public void GroupBySelectManyTest()
		{
			var result = db.Customers
				.GroupBy(c => c.Address.City)
				.SelectMany(g => g);

			var list = result.ToList();
			Assert.AreEqual(91, list.Count);
		}

		[Test]
		[Category("Grouping")]
		// Passed.
		public void GroupByCalculateAggregateTest()
		{
			var result =
				from o in db.Orders
				group o by o.Customer
				into g
				select g.Sum(o => o.Freight);

			var list = result.ToList();
			Assert.AreEqual(89, list.Count);
		}

		[Test]
		[Category("Grouping")]
		// Passed.
		public void GroupByCalculateManyAggreagetes()
		{
			var result =
				from o in db.Orders
				group o by o.Customer
				into g
				select new
						   {
							   Sum = g.Sum(o => o.Freight),
							   Min = g.Min(o => o.Freight),
							   Max = g.Max(o => o.Freight),
							   Avg = g.Average(o => o.Freight)
						   };

			var list = result.ToList();
			Assert.AreEqual(89, list.Count);
		}

		[Test]
		[Category("Grouping")]
		//[ExpectedException(typeof(NullReferenceException))]
		// Failed.
		// Exception: NullReferenceException
		// Message:
		//   at NHibernate.Linq.Visitors.ResultOperatorProcessors.ProcessNonAggregatingGroupBy.SourceOf(Expression keySelector) in ProcessNonAggregatingGroupBy.cs: line 74
		public void GroupByAggregate()
		{
			var result =
				from c in db.Customers
				group c by c.Orders.Average(o => o.Freight) >= 80;
			var list = result.ToList();
			Assert.AreEqual(2, list.Count);
			var firstGroupList = list.First(g => !g.Key).ToList();
			Assert.AreEqual(71, firstGroupList.Count);
		}

		[Test]
		[Category("Grouping")]
		//[ExpectedException(typeof(HibernateException))]
		// Failed.
		// Exception: HibernateException 
		// Message:
		//   Query Source could not be identified: ItemName = mg, ItemType = System.Linq.IGrouping`2[System.Int32,NHibernate.DomainModel.Northwind.Entities.Order], Expression = from IGrouping`2 mg in {[o] => GroupBy(Convert([o].OrderDate).Month, [o])}
		public void ComplexGroupingTest()
		{
			var result =
				from c in db.Customers
				select new
						   {
							   c.CompanyName,
							   YearGroups =
					from o in c.Orders
					group o by o.OrderDate.Value.Year
					into yg
					select new
							   {
								   Year = yg.Key,
								   MonthGroups =
						from o in yg
						group o by o.OrderDate.Value.Month
						into mg
						select new {Month = mg.Key, Orders = mg}
							   }
						   };
			var list = result.ToList();
			foreach (var customer in list)
			{
				var ordersList = customer.YearGroups.ToList();
				Assert.LessOrEqual(ordersList.Count, 3);
			}
		}

		#endregion

		#region Set operations / Distinct tests

		[Test]
		[Category("Set operations")]
		//[ExpectedException(typeof(ParserException))]
		// Failed.
		// Exception: ParserException
		// Message:
		//   Could not parse expression 'value(NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Customer]).Where(c => (c.Orders.Count <= 1)).Concat(value(NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Customer]).Where(c => (c.Orders.Count > 1)))': This overload of the method 'System.Linq.Queryable.Concat' is currently not supported.
		public void ConcatTest()
		{
			var result = db.Customers.Where(c => c.Orders.Count <= 1).Concat(db.Customers.Where(c => c.Orders.Count > 1));
			var list = result.ToList();
			Assert.AreEqual(91, list.Count);
		}

		[Test]
		[Category("Set operations")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   The UnionResultOperator result operator is not current supported
		public void UnionTest()
		{
			var result = (
							 from c in db.Customers
							 select c.Address.PhoneNumber)
				.Union(
					from c in db.Customers
					select c.Address.Fax)
				.Union(
					from e in db.Employees
					select e.Address.PhoneNumber
				);

			var list = result.ToList();
			Assert.AreEqual(167, list.Count);
		}

		[Test]
		[Category("Set operations")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   The ExceptResultOperator result operator is not current supported
		public void ExceptTest()
		{
			var result =
				db.Customers.Except(db.Customers.Where(c => c.Orders.Count() > 0));
			var list = result.ToList();
			Assert.AreEqual(2, list.Count);
		}

		[Test]
		[Category("Set operations")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   The IntersectResultOperator result operator is not current supported
		public void IntersectTest()
		{
			var result =
				db.Customers.Intersect(db.Customers.Where(c => c.Orders.Count() > 0));
			var list = result.ToList();
			Assert.AreEqual(89, list.Count);
		}

		[Test]
		[Category("Set operations")]
		// Passed.
		public void DistinctTest()
		{
			var result = db.Orders.Select(c => c.Freight).Distinct();
			var list = result.ToList();
			Assert.AreEqual(799, list.Count);
		}

		[Test]
		[Category("Set operations")]
		// Passed.
		public void DistinctTakeLastTest()
		{
			var result =
				(from o in db.Orders
				 orderby o.OrderDate
				 select o.OrderDate).Distinct().Take(5);
			var list = result.ToList();
			Assert.AreEqual(5, list.Count);
		}

		[Test]
		[Category("Set operations")]
		//[ExpectedException(typeof(AssertionException))]
		// Failed with assertion.
		// Exception: AssertionException
		// Message:
		//     Expected: 4
		//     But was:  5
		public void DistinctTakeFirstTest()
		{
			var result =
				(from o in db.Orders
				 orderby o.OrderDate
				 select o.OrderDate).Take(5).Distinct();
			var list = result.ToList();
			Assert.AreEqual(4, list.Count);
		}

		[Test]
		[Category("Set operations")]
		// Passed.
		public void DistinctEntityTest()
		{
			var result = db.Customers.Distinct();
			var list = result.ToList();
			Assert.AreEqual(91, list.Count);
		}

		[Test]
		[Category("Set operations")]
		// Passed.
		public void DistinctAnonymousTest()
		{
			var result = db.Customers.Select(c => new {c.Address.Region, c.Address.City}).Distinct();
			var list = result.ToList();
			Assert.AreEqual(69, list.Count);
		}

		#endregion

		#region Type casts

		/*
			[Test]
			[Category("Type casts")]
			// Passed.
			public void TypeCastIsChildTest()
			{
				var result = db.Products.Where(p => p is DiscontinuedProduct);
				var expected = db.Products.ToList().Where(p => p is DiscontinuedProduct);
				var list = result.ToList();
				Assert.Greater(list.Count, 0);
				Assert.AreEqual(expected.Count(), list.Count);
			}
*/

		[Test]
		[Category("Type casts")]
		//[ExpectedException(typeof(QueryException))]
		// Failed.
		// Exception: QueryException
		// Message:
		//   could not resolve property: class of: NHibernate.DomainModel.Northwind.Entities.Product [.Where[NHibernate.DomainModel.Northwind.Entities.Product](NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Product], Quote((p, ) => (IsType(p, NHibernate.DomainModel.Northwind.Entities.Product))), )]
		public void TypeCastIsParentTest()
		{
			var result = db.Products.Where(p => p is Product);
			var expected = db.Products.ToList();
			var list = result.ToList();
			Assert.Greater(list.Count, 0);
			Assert.AreEqual(expected.Count(), list.Count);
		}

		/*
					[Test]
					[Category("Type casts")]
					// Passed.
					public void TypeCastIsChildConditionalTest()
					{
						var result = db.Products
						  .Select(x => x is DiscontinuedProduct
										 ? x
										 : null);

						var expected = db.Products.ToList()
						  .Select(x => x is DiscontinuedProduct
										 ? x
										 : null);

						var list = result.ToList();
						Assert.Greater(list.Count, 0);
						Assert.AreEqual(expected.Count(), list.Count);
						Assert.IsTrue(list.Except(expected).Count() == 0);
						Assert.IsTrue(list.Contains(null));
					}

					[Test]
					[Category("Type casts")]
					// Passed.
					public void TypeCastOfTypeTest()
					{
						var result = db.Products.OfType<DiscontinuedProduct>();
						var expected = db.Products.ToList().OfType<DiscontinuedProduct>();
						var list = result.ToList();
						Assert.Greater(list.Count, 0);
						Assert.AreEqual(expected.Count(), list.Count);
					}

					[Test]
					[Category("Type casts")]
					// Failed.
					// Exception: NotSupportedException
					// Message:
					//   ([100001] As Product)
					public void TypeCastAsTest()
					{
						var result = db.DiscontinuedProducts
						  .Select(discontinuedProduct => discontinuedProduct as Product)
						  .Select(product =>
								  product == null
									? "NULL"
									: product.ProductName);

						var expected = db.DiscontinuedProducts.ToList()
						  .Select(discontinuedProduct => discontinuedProduct as Product)
						  .Select(product =>
								  product == null
									? "NULL"
									: product.ProductName);

						var list = result.ToList();
						Assert.Greater(list.Count, 0);
						Assert.AreEqual(expected.Count(), list.Count);
						Assert.IsTrue(list.Except(expected).Count() == 0);
					}
		*/

		#endregion

		#region Element operations

		[Test]
		[Category("Element operations")]
		// Passed.
		public void FirstTest()
		{
			var customer = db.Customers.First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Element operations")]
		// Passed.
		public void FirstOrDefaultTest()
		{
			var customer = db.Customers.Where(c => c.CustomerId == "ALFKI").FirstOrDefault();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Element operations")]
		// Passed.
		public void FirstPredicateTest()
		{
			var customer = db.Customers.First(c => c.CustomerId == "ALFKI");
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Element operations")]
		//[ExpectedException(typeof(QuerySyntaxException))]
		// Failed.
		// Exception: QuerySyntaxException
		// Message:
		//   Exception of type 'Antlr.Runtime.NoViableAltException' was thrown. [.Select[OrmBattle.NHibernateModel.Northwind.Product,<>f__AnonymousType10`2[[System.Int32, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[OrmBattle.NHibernateModel.Northwind.Order, NHibernateModel, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]](NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Product], Quote((p, ) => (new <>f__AnonymousType10`2(p.Id, .FirstOrDefault[OrmBattle.NHibernateModel.Northwind.OrderDetail](.OrderByDescending[OrmBattle.NHibernateModel.Northwind.OrderDetail,System.Decimal](.Where[OrmBattle.NHibernateModel.Northwind.OrderDetail](NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.OrderDetail], Quote((od, ) => (Equal(od.Product, p))), ), Quote((od, ) => (Decimal.op_Multiply(od.UnitPrice, Convert(od.Quantity)))), ), ).Order, ))), )]
		public void NestedFirstOrDefaultTest()
		{
			var result =
				from p in db.Products
				select new
						   {
							   ProductID = p.ProductId,
							   MaxOrder = db.OrderLines
					.Where(od => od.Product == p)
					.OrderByDescending(od => od.UnitPrice*od.Quantity)
					.FirstOrDefault()
					.Order
						   };
			var list = result.ToList();
			Assert.Greater(list.Count, 0);
		}

		[Test]
		[Category("Element operations")]
		// Passed.
		public void FirstOrDefaultEntitySetTest()
		{
			var customersCount = Customers.Count;
			var result = db.Customers.Select(c => c.Orders.FirstOrDefault());
			var list = result.ToList();
			Assert.AreEqual(customersCount, list.Count);
		}

		[Test]
		[Category("Element operations")]
		// Passed.
		public void NestedSingleOrDefaultTest()
		{
			var customersCount = Customers.Count;
			var result = db.Customers.Select(c => c.Orders.Take(1).SingleOrDefault());
			var list = result.ToList();
			Assert.AreEqual(customersCount, list.Count);
		}

		[Test]
		[Category("Element operations")]
		// Passed.
		public void NestedSingleTest()
		{
			var result = db.Customers.Where(c => c.Orders.Count() > 0).Select(c => c.Orders.Take(1).Single());
			var list = result.ToList();
			Assert.Greater(list.Count, 0);
		}

		[Test]
		[Category("Element operations")]
		//[ExpectedException(typeof(ParserException))]
		// Failed.
		// Exception: ParserException
		// Message:
		//   Could not parse expression 'value(NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Customer]).OrderBy(c => c.Id).ElementAt(15)': This overload of the method 'System.Linq.Queryable.ElementAt' is currently not supported.
		public void ElementAtTest()
		{
			var customer = db.Customers.OrderBy(c => c.CustomerId).ElementAt(15);
			Assert.IsNotNull(customer);
			Assert.AreEqual("CONSH", customer.CustomerId);
		}

		[Test]
		[Category("Element operations")]
		// Passed.
		public void NestedElementAtTest()
		{
			var result =
				from c in db.Customers
				where c.Orders.Count() > 5
				select c.Orders.ElementAt(3);

			var list = result.ToList();
			Assert.AreEqual(63, list.Count);
		}

		#endregion


		#region Contains / Any / All tests

		[Test]
		[Category("All/Any/Contains")]
		// Passed.
		public void AllNestedTest()
		{
			var result =
				from c in db.Customers
				where db.Orders.Where(o => o.Customer == c).All(o => db.Employees.Where(e => o.Employee == e).Any(e => e.FirstName.StartsWith("A")))
				select c;
			var list = result.ToList();
			Assert.AreEqual(2, list.Count);
		}

		[Test]
		[Category("All/Any/Contains")]
		// Passed.
		public void ComplexAllTest()
		{
			var result =
				from o in db.Orders
				where
					db.Customers.Where(c => c == o.Customer).All(c => c.CompanyName.StartsWith("A")) ||
					db.Employees.Where(e => e == o.Employee).All(e => e.FirstName.EndsWith("t"))
				select o;
			var expected =
				from o in Orders
				where
					Customers.Where(c => c == o.Customer).All(c => c.CompanyName.StartsWith("A")) ||
					Employees.Where(e => e == o.Employee).All(e => e.FirstName.EndsWith("t"))
				select o;

			Assert.AreEqual(0, expected.Except(result).Count());
			Assert.AreEqual(result.ToList().Count, 366);
		}

		[Test]
		[Category("All/Any/Contains")]
		// Passed.
		public void ContainsNestedTest()
		{
			var result = from c in db.Customers
						 select new
									{
										Customer = c,
										HasNewOrders = db.Orders
							 .Where(o => o.OrderDate > new DateTime(2001, 1, 1))
							 .Select(o => o.Customer)
							 .Contains(c)
									};

			var expected =
				from c in Customers
				select new
						   {
							   Customer = c,
							   HasNewOrders = Orders
					.Where(o => o.OrderDate > new DateTime(2001, 1, 1))
					.Select(o => o.Customer)
					.Contains(c)
						   };
			Assert.AreEqual(0, expected.Except(result).Count());
			Assert.AreEqual(0, result.ToList().Count(i => i.HasNewOrders));
		}

		[Test]
		[Category("All/Any/Contains")]
		// Passed.
		public void AnyTest()
		{
			var result = db.Customers.Where(c => c.Orders.Any(o => o.Freight > 400));
			var expected = Customers.Where(c => c.Orders.Any(o => o.Freight > 400));
			Assert.AreEqual(0, expected.Except(result).Count());
			Assert.AreEqual(10, result.ToList().Count);
		}

		[Test]
		[Category("All/Any/Contains")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   Specified method is not supported.
		public void AnyParameterizedTest()
		{
			var ids = new[] {"ABCDE", "ALFKI"};
			var result = db.Customers.Where(c => ids.Any(id => c.CustomerId == id));
			var list = result.ToList();
			Assert.Greater(list.Count, 0);
		}

		[Test]
		[Category("All/Any/Contains")]
		// Passed.
		public void ContainsParameterizedTest()
		{
			var customerIDs = new[] {"ALFKI", "ANATR", "AROUT", "BERGS"};
			var result = db.Orders.Where(o => customerIDs.Contains(o.Customer.CustomerId));
			var list = result.ToList();
			Assert.Greater(list.Count, 0);
			Assert.AreEqual(41, list.Count);
		}

		#endregion

		#region Aggregates tests

		[Test]
		[Category("Aggregates")]
		// Passed.
		public void SumTest()
		{
			var sum = db.Orders.Select(o => o.Freight).Sum();
			var sum1 = Orders.Select(o => o.Freight).Sum();
			Assert.AreEqual(sum1, sum);
		}

		[Test]
		[Category("Aggregates")]
		// Passed.
		public void CountPredicateTest()
		{
			var count = db.Orders.Count(o => o.OrderId > 10);
			var count1 = Orders.Count(o => o.OrderId > 10);
			Assert.AreEqual(count1, count);
		}

		[Test]
		[Category("Aggregates")]
		// Passed.
		public void NestedCountTest()
		{
			var result = db.Customers.Where(c => db.Orders.Count(o => o.Customer.CustomerId == c.CustomerId) > 5);
			var expected = Customers.Where(c => db.Orders.Count(o => o.Customer.CustomerId == c.CustomerId) > 5);

			Assert.IsTrue(expected.Except(result).Count() == 0);
		}

		[Test]
		[Category("Aggregates")]
		// Passed.
		public void NullableSumTest()
		{
			var sum = db.Orders.Select(o => (int?) o.OrderId).Sum();
			var sum1 = Orders.Select(o => (int?) o.OrderId).Sum();
			Assert.AreEqual(sum1, sum);
		}

		[Test]
		[Category("Aggregates")]
		//[ExpectedException(typeof(QuerySyntaxException))]
		// Failed.
		// Exception: QuerySyntaxException
		// Message:
		//   Exception of type 'Antlr.Runtime.NoViableAltException' was thrown. [.Max[OrmBattle.NHibernateModel.Northwind.Customer,System.Int32](NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Customer], Quote((c, ) => (.Count[OrmBattle.NHibernateModel.Northwind.Order](NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Order], Quote((o, ) => (String.op_Equality(o.Customer.Id, c.Id))), ))), )]
		public void MaxCountTest()
		{
			var max = db.Customers.Max(c => db.Orders.Count(o => o.Customer.CustomerId == c.CustomerId));
			var max1 = Customers.Max(c => db.Orders.Count(o => o.Customer.CustomerId == c.CustomerId));
			Assert.AreEqual(max1, max);
		}

		#endregion

		#region Join tests

		[Test]
		[Category("Join")]
		// Passed.
		public void GroupJoinTest()
		{
			var result =
				from c in db.Customers
				join o in db.Orders on c.CustomerId equals o.Customer.CustomerId into go
				join e in db.Employees on c.Address.City equals e.Address.City into ge
				select new
						   {
							   OrdersCount = go.Count(),
							   EmployeesCount = ge.Count()
						   };
			var list = result.ToList();
			Assert.AreEqual(91, list.Count);
		}

		[Test]
		[Category("Join")]
		// Passed.
		public void JoinTest()
		{
			var result =
				from p in db.Products
				join s in db.Suppliers on p.Supplier.SupplierId equals s.SupplierId
				select new {ProductName = p.Name, s.ContactName, s.Address.PhoneNumber};

			var list = result.ToList();
			Assert.Greater(list.Count, 0);
		}

		[Test]
		[Category("Join")]
		// Passed.
		public void JoinByAnonymousTest()
		{
			var result =
				from c in db.Customers
				join o in db.Orders on new {Customer = c, Name = c.ContactName} equals
					new {o.Customer, Name = o.Customer.ContactName}
				select new {c.ContactName, o.OrderDate};

			var list = result.ToList();
			Assert.Greater(list.Count, 0);
		}

		[Test]
		[Category("Join")]
		// Passed.
		public void LeftJoinTest()
		{
			var result =
				from c in db.Categories
				join p in db.Products on c.CategoryId equals p.Category.CategoryId into g
				from p in g.DefaultIfEmpty()
				select new {Name = p == null ? "Nothing!" : p.Name, CategoryName = c.Name};

			var list = result.ToList();
			Assert.AreEqual(77, list.Count);
		}

		#endregion

		#region References tests

		[Test]
		[Category("References")]
		// Passed.
		public void JoinByReferenceTest()
		{
			var result =
				from c in db.Customers
				join o in db.Orders on c equals o.Customer
				select new {c.ContactName, o.OrderDate};

			var list = result.ToList();
			Assert.AreEqual(830, list.Count);
		}

		[Test]
		[Category("References")]
		// Passed.
		public void CompareReferenceTest()
		{
			var result =
				from c in db.Customers
				from o in db.Orders
				where c == o.Customer
				select new {c.ContactName, o.OrderDate};

			var list = result.ToList();
			Assert.AreEqual(830, list.Count);
		}

		[Test]
		[Category("References")]
		// Passed.
		public void ReferenceNavigationTestTest()
		{
			var result =
				from od in db.OrderLines
				where od.Product.Category.Name == "Seafood"
				select new {od.Order, od.Product};

			var list = result.ToList();
			Assert.AreEqual(330, list.Count);
			foreach (var anonymous in list)
			{
				Assert.IsNotNull(anonymous);
				Assert.IsNotNull(anonymous.Order);
				Assert.IsNotNull(anonymous.Product);
			}
		}

		[Test]
		[Category("References")]
		//[ExpectedException(typeof(QueryException))]
		// Failed.
		// Exception: QueryException 
		// Message:
		//   could not resolve property: Count of: NHibernate.DomainModel.Northwind.Entities.Product [.Where[NHibernate.DomainModel.Northwind.Entities.ProductCategory](NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.ProductCategory], Quote((c, ) => (GreaterThan(c.Products.Count, p1))), )]       
		public void EntitySetCountTest()
		{
			var result = db.Categories.Where(c => c.Products.Count > 10);
			var list = result.ToList();
			Assert.AreEqual(4, list.Count);
		}

		#endregion


		#region Complex tests

		[Test]
		[Category("Complex")]
		//[ExpectedException(typeof(ArgumentException))]
		// Failed.
		// Exception: GenericADOException
		// Message:
		//   Could not execute query[SQL: SQL not available]
		public void ComplexTest1()
		{
			var result = db.Suppliers.Select(
				supplier => db.Products.Select(
					product => db.Products.Where(p => p.ProductId == product.ProductId && p.Supplier.SupplierId == supplier.SupplierId)));
			var count = result.ToList().Count;
			Assert.Greater(count, 0);
			foreach (var queryable in result)
			{
				foreach (var queryable1 in queryable)
				{
					foreach (var product in queryable1)
					{
						Assert.IsNotNull(product);
					}
				}
			}
		}

		[Test]
		[Category("Complex")]
		//[ExpectedException(typeof(HibernateException))]
		// Failed.
		// Exception: HibernateException 
		// Message:
		//   Query Source could not be identified: ItemName = k, ItemType = System.Linq.IGrouping`2[System.String,NHibernate.DomainModel.Northwind.Entities.Customer], Expression = from IGrouping`2 k in {value(NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Customer]) => GroupBy([c].Address.Country, [c])}
		public void ComplexTest2()
		{
			var result = db.Customers
				.GroupBy(c => c.Address.Country, (country, customers) => customers.Where(k => k.CompanyName.Substring(0, 1) == country.Substring(0, 1)))
				.SelectMany(k => k);
			var expected = Customers
				.GroupBy(c => c.Address.Country, (country, customers) => customers.Where(k => k.CompanyName.Substring(0, 1) == country.Substring(0, 1)))
				.SelectMany(k => k);

			Assert.AreEqual(0, expected.Except(result).Count());
		}

		[Test]
		[Category("Complex")]
		//[ExpectedException(typeof(ArgumentException))]
		// Failed.
		// Inner Exception: InvalidCastException
		// Message:
		//   Unable to cast object of type 'System.String' to type 'System.Linq.IQueryable`1[System.String]'.
		public void ComplexTest3()
		{
			var products = db.Products;
			var suppliers = db.Suppliers;
			var result = from p in products
						 select new
									{
										Product = p,
										Suppliers = suppliers
							 .Where(s => s.SupplierId == p.Supplier.SupplierId)
							 .Select(s => s.CompanyName)
									};
			var list = result.ToList();
			Assert.Greater(list.Count, 0);
			foreach (var p in list)
				foreach (var companyName in p.Suppliers)
					Assert.IsNotNull(companyName);
		}

		[Test]
		[Category("Complex")]
		//[ExpectedException(typeof(ArgumentException))]
		// Failed.
		// Exception: QuerySyntaxException
		// Message:
		//   e.Orders is not mapped [.Select[System.Linq.IQueryable`1[[System.Linq.IQueryable`1[[NHibernate.DomainModel.Northwind.Entities.Employee, NHibernate.DomainModel, Version=3.3.0.3001, Culture=neutral, PublicKeyToken=null]], System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]],System.Linq.IQueryable`1[[System.Linq.IQueryable`1[[NHibernate.DomainModel.Northwind.Entities.Employee, NHibernate.DomainModel, Version=3.3.0.3001, Culture=neutral, PublicKeyToken=null]], System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]](.Select[NHibernate.DomainModel.Northwind.Entities.Customer,System.Linq.IQueryable`1[[System.Linq.IQueryable`1[[NHibernate.DomainModel.Northwind.Entities.Employee, NHibernate.DomainModel, Version=3.3.0.3001, Culture=neutral, PublicKeyToken=null]], System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]](.Take[NHibernate.DomainModel.Northwind.Entities.Customer](NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Customer], p1, ), Quote((c, ) => (.Where[System.Linq.IQueryable`1[[NHibernate.DomainModel.Northwind.Entities.Employee, NHibernate.DomainModel, Version=3.3.0.3001, Culture=neutral, PublicKeyToken=null]]](.Select[NHibernate.DomainModel.Northwind.Entities.Order,System.Linq.IQueryable`1[[NHibernate.DomainModel.Northwind.Entities.Employee, NHibernate.DomainModel, Version=3.3.0.3001, Culture=neutral, PublicKeyToken=null]]](NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Order], Quote((o, ) => (.Where[NHibernate.DomainModel.Northwind.Entities.Employee](.Take[NHibernate.DomainModel.Northwind.Entities.Employee](NHibernate.Linq.NhQueryable`1[NHibernate.DomainModel.Northwind.Entities.Employee], p2, ), Quote((e, ) => (e.Orders.Contains(o, ))), ))), ), Quote((o, ) => (GreaterThan(.Count[NHibernate.DomainModel.Northwind.Entities.Employee](o, ), p3))), ))), ), Quote((os, ) => (os)), )]
		public void ComplexTest4()
		{
			var result = db.Customers
				.Take(2)
				.Select(c => db.Orders.Select(o => db.Employees.Take(2).Where(e => e.Orders.Contains(o))).Where(o => o.Count() > 0))
				.Select(os => os);

			var list = result.ToList();
			Assert.Greater(list.Count, 0);

			foreach (var item in list)
				item.ToList();
		}

		[Test]
		[Category("Complex")]
		// Passed
		public void ComplexTest5()
		{
			var result = db.Customers
				.Select(c => new {Customer = c, Orders = db.Orders})
				.Select(i => i.Customer.Orders);

			var list = result.ToList();
			Assert.Greater(list.Count, 0);

			foreach (var item in list)
				item.ToList();
		}

		[Test]
		[Category("Complex")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   Specified method is not supported.
		public void ComplexTest6()
		{
			var result = db.Customers
				.Select(c => new {Customer = c, Orders = db.Orders.Where(o => o.Customer == c)})
				.SelectMany(i => i.Orders.Select(o => new {i.Customer, Order = o}));

			var list = result.ToList();
			Assert.Greater(list.Count, 0);
		}

		#endregion

		#region Standard functions tests

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void StringStartsWithTest()
		{
			var result = db.Customers.Where(c => c.CustomerId.StartsWith("A") || c.CustomerId.StartsWith("L"));

			var list = result.ToList();
			Assert.AreEqual(13, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void StringStartsWithParameterizedTest()
		{
			var likeA = "A";
			var likeL = "L";
			var result = db.Customers.Where(c => c.CustomerId.StartsWith(likeA) || c.CustomerId.StartsWith(likeL));

			var list = result.ToList();
			Assert.AreEqual(13, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void StringLengthTest()
		{
			var customer = db.Customers.Where(c => c.Address.City.Length == 7).First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void StringContainsTest()
		{
			var customer = db.Customers.Where(c => c.ContactName.Contains("and")).First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void StringToLowerTest()
		{
			var customer = db.Customers.Where(c => c.Address.City.ToLower() == "seattle").First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   System.String Remove(Int32)
		public void StringRemoveTest()
		{
			var customer = db.Customers.Where(c => c.Address.City.Remove(3) == "Sea").First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void StringIndexOfTest()
		{
			var customer = db.Customers.Where(c => c.Address.City.IndexOf("tt") == 3).First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   Int32 LastIndexOf(System.String, Int32, Int32)
		public void StringLastIndexOfTest()
		{
			var customer = db.Customers.Where(c => c.Address.City.LastIndexOf("t", 1, 3) == 3).First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   System.String PadLeft(Int32)
		public void StringPadLeftTest()
		{
			var customer = db.Customers.Where(c => "123" + c.Address.City.PadLeft(8) == "123 Seattle").First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   NewExpression
		public void DateTimeTest()
		{
			var order = db.Orders.Where(o => o.OrderDate >= new DateTime(o.OrderDate.Value.Year, 1, 1)).First();
			Assert.IsNotNull(order);
		}

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void DateTimeDayTest()
		{
			var order = db.Orders.Where(o => o.OrderDate.Value.Day == 5).First();
			Assert.IsNotNull(order);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(QueryException))]
		// Failed.
		// Exception: QueryException
		// Message:
		//   could not resolve property: DayOfWeek of: OrmBattle.NHibernateModel.Northwind.Order [.First[OrmBattle.NHibernateModel.Northwind.Order](.Where[OrmBattle.NHibernateModel.Northwind.Order](NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Order], Quote((o, ) => (Equal(Convert(o.OrderDate.Value.DayOfWeek), p1))), ), )]
		public void DateTimeDayOfWeek()
		{
			var order = db.Orders.Where(o => o.OrderDate.Value.DayOfWeek == DayOfWeek.Friday).First();
			Assert.IsNotNull(order);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(QueryException))]
		// Failed.
		// Exception: QueryException
		// Message:
		//   could not resolve property: DayOfYear of: OrmBattle.NHibernateModel.Northwind.Order [.First[OrmBattle.NHibernateModel.Northwind.Order](.Where[OrmBattle.NHibernateModel.Northwind.Order](NHibernate.Linq.NhQueryable`1[OrmBattle.NHibernateModel.Northwind.Order], Quote((o, ) => (Equal(o.OrderDate.Value.DayOfYear, p1))), ), )]
		public void DateTimeDayOfYear()
		{
			var order = db.Orders.Where(o => o.OrderDate.Value.DayOfYear == 360).First();
			Assert.IsNotNull(order);
		}

		[Test]
		[Category("Standard functions")]
		// Passed
		public void MathAbsTest()
		{
			var order = db.Orders.Where(o => Math.Abs(o.OrderId) == 10 || o.OrderId > 0).First();
			Assert.IsNotNull(order);
		}

		[Test]
		[Category("Standard functions")]
		// Passed
		public void MathTrigonometricTest()
		{
			var order = db.Orders.Where(o => Math.Asin(Math.Cos(o.OrderId)) == 0 || o.OrderId > 0).First();
			Assert.IsNotNull(order);
		}

		[Test]
		[Category("Standard functions")]
		// Passed
		public void MathFloorTest()
		{
			var result = db.Orders.Where(o => Math.Floor(o.Freight.GetValueOrDefault()) == 140);
			var list = result.ToList();
			Assert.AreEqual(2, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		// Passed
		public void MathCeilingTest()
		{
			var result = db.Orders.Where(o => Math.Ceiling(o.Freight.GetValueOrDefault()) == 141);
			var list = result.ToList();
			Assert.AreEqual(2, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		// Passed
		public void MathTruncateTest()
		{
			var result = db.Orders.Where(o => Math.Truncate(o.Freight.GetValueOrDefault()) == 141);
			var list = result.ToList();
			Assert.AreEqual(2, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   System.Decimal Round(System.Decimal, Int32, System.MidpointRounding)
		public void MathRoundAwayFromZeroTest()
		{
			var result = db.Orders.Where(o => Math.Round(o.Freight.GetValueOrDefault()/10, 1, MidpointRounding.AwayFromZero) == 6.5m);
			var list = result.ToList();
			Assert.AreEqual(7, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   System.Decimal Round(System.Decimal, Int32, System.MidpointRounding)
		public void MathRoundToEvenTest()
		{
			var result = db.Orders.Where(o => Math.Round(o.Freight.GetValueOrDefault()/10, 1, MidpointRounding.ToEven) == 6.5m);
			var list = result.ToList();
			Assert.AreEqual(6, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   System.Decimal Round(System.Decimal, Int32)
		public void MathRoundDefaultTest()
		{
			var result = db.Orders.Where(o => Math.Round(o.Freight.GetValueOrDefault()/10, 1) == 6.5m);
			var list = result.ToList();
			Assert.AreEqual(6, list.Count);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Passed.
		public void ConvertToInt32()
		{
			var expected = Orders.Where(o => Convert.ToInt32(o.Freight*10) == 592);
			var result = db.Orders.Where(o => Convert.ToInt32(o.Freight*10) == 592);
			var list = result.ToList();
			Assert.AreEqual(expected.Count(), list.Count);
		}

		[Test]
		[Category("Standard functions")]
		// Failed.
		// Exception: NotSupportedException
		// Message:
		//   Int32 CompareTo(System.String)
		public void StringCompareToTest()
		{
			var customer = db.Customers.Where(c => c.Address.City.CompareTo("Seattle") >= 0).First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		// Passed.
		public void ComparisonWithNullTest()
		{
			var customer = db.Customers.Where(c => null != c.Address.City).First();
			Assert.IsNotNull(customer);
		}

		[Test]
		[Category("Standard functions")]
		//[ExpectedException(typeof(NotSupportedException))]
		// Failed.
		// Exception: NotSupportedException 
		// Message:
		//   Boolean Equals(System.Object)
		public void EqualsWithNullTest()
		{
			var customer = db.Customers.Where(c => !c.Address.Equals(null)).First();
			Assert.IsNotNull(customer);
		}

		#endregion
	}
}
