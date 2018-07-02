﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Dynamic.Core;
using System.Linq;
using Antlr.Runtime.Misc;
using NUnit.Framework;
using NHibernate.Criterion;
using NHibernate.Linq;

namespace NHibernate.Test.EntityModeTest.Map.Basic
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class DynamicClassFixtureAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override IList Mappings
		{
			get { return new[] {"EntityModeTest.Map.Basic.ProductLine.hbm.xml"}; }
		}

		public delegate IDictionary SingleCarQueryDelegate(ISession session);
		public delegate IList AllModelQueryDelegate(ISession session);

		[Test]
		public async Task ShouldWorkWithHQLAsync()
		{
			await (TestLazyDynamicClassAsync(
				s => (IDictionary) s.CreateQuery("from ProductLine pl order by pl.Description").UniqueResult(),
				s => s.CreateQuery("from Model m").List()));
		}

		[Test]
		public async Task ShouldWorkWithCriteriaAsync()
		{
			await (TestLazyDynamicClassAsync(
				s => (IDictionary) s.CreateCriteria("ProductLine").AddOrder(Order.Asc("Description")).UniqueResult(),
				s => s.CreateCriteria("Model").List()));
		}

		public async Task TestLazyDynamicClassAsync(SingleCarQueryDelegate singleCarQueryHandler, AllModelQueryDelegate allModelQueryHandler, CancellationToken cancellationToken = default(CancellationToken))
		{
			ITransaction t;
			IDictionary cars;
			IList models;
			using (ISession s = OpenSession())
			{
				t = s.BeginTransaction();

				cars = new Hashtable();
				cars["Description"] = "Cars";

				IDictionary monaro = new Hashtable();
				monaro["ProductLine"] = cars;
				monaro["Name"] = "Monaro";
				monaro["Description"] = "Holden Monaro";

				IDictionary hsv = new Hashtable();
				hsv["ProductLine"] = cars;
				hsv["Name"] = "hsv";
				hsv["Description"] = "Holden hsv";

				models = new List<IDictionary> {monaro, hsv};

				cars["Models"] = models;

				await (s.SaveAsync("ProductLine", cars, cancellationToken));
				await (t.CommitAsync(cancellationToken));
			}

			using (ISession s = OpenSession())
			{
				t = s.BeginTransaction();
				cars = singleCarQueryHandler(s);
				models = (IList)cars["Models"];
				Assert.IsFalse(NHibernateUtil.IsInitialized(models));
				Assert.AreEqual(2, models.Count);
				Assert.IsTrue(NHibernateUtil.IsInitialized(models));
				s.Clear();
				IList list = allModelQueryHandler(s);
				foreach (IDictionary ht in list)
				{
					Assert.IsFalse(NHibernateUtil.IsInitialized(ht["ProductLine"]));
				}
				var model = (IDictionary)list[0];
				Assert.IsTrue(((IList)((IDictionary)model["ProductLine"])["Models"]).Contains(model));
				s.Clear();

				await (t.CommitAsync(cancellationToken));
			}

			using (ISession s = OpenSession())
			{
				t = s.BeginTransaction();
				cars = singleCarQueryHandler(s);
				await (s.DeleteAsync(cars, cancellationToken));
				await (t.CommitAsync(cancellationToken));
			}
		}

		[Test]
		public async Task ShouldWorkWithHQLAndGenericsAsync()
		{
			await (TestLazyDynamicClassAsync(
				s => s.CreateQuery("from ProductLine pl order by pl.Description").UniqueResult<IDictionary<string, object>>(),
				s => s.CreateQuery("from Model m").List<IDictionary<string, object>>()));
		}

		[Test]
		public async Task ShouldWorkWithCriteriaAndGenericsAsync()
		{
			await (TestLazyDynamicClassAsync(
				s => s.CreateCriteria("ProductLine").AddOrder(Order.Asc("Description")).UniqueResult<IDictionary<string, object>>(),
				s => s.CreateCriteria("Model").List<IDictionary<string, object>>()));
		}

		[Test]
		public async Task ShouldWorkWithLinqAndGenericsAsync()
		{
			await (TestLazyDynamicClassAsync(
				s => (IDictionary<string, object>) s.Query<dynamic>("ProductLine").OrderBy("Description").Single(),
				s => s.Query<dynamic>("Model").ToList().Cast<IDictionary<string, object>>().ToList()));
		}

		public async Task TestLazyDynamicClassAsync(
			Func<ISession, IDictionary<string, object>> singleCarQueryHandler,
			Func<ISession, IList<IDictionary<string, object>>> allModelQueryHandler, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var cars = new Dictionary<string, object> { ["Description"] = "Cars" };

				var monaro = new Dictionary<string, object>
				{
					["ProductLine"] = cars,
					["Name"] = "Monaro",
					["Description"] = "Holden Monaro"
				};

				var hsv = new Dictionary<string, object>
				{
					["ProductLine"] = cars,
					["Name"] = "hsv",
					["Description"] = "Holden hsv"
				};

				var models = new List<IDictionary<string, object>> {monaro, hsv};

				cars["Models"] = models;

				await (s.SaveAsync("ProductLine", cars, cancellationToken));
				await (t.CommitAsync(cancellationToken));
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var cars = singleCarQueryHandler(s);
				var models = (IList<object>) cars["Models"];
				Assert.That(NHibernateUtil.IsInitialized(models), Is.False);
				Assert.That(models.Count, Is.EqualTo(2));
				Assert.That(NHibernateUtil.IsInitialized(models), Is.True);
				s.Clear();
				var list = allModelQueryHandler(s);
				foreach (var dic in list)
				{
					Assert.That(NHibernateUtil.IsInitialized(dic["ProductLine"]), Is.False);
				}
				var model = list[0];
				Assert.That(((IList<object>) ((IDictionary<string, object>) model["ProductLine"])["Models"]).Contains(model), Is.True);
				s.Clear();

				await (t.CommitAsync(cancellationToken));
			}
		}

		[Test]
		public async Task ShouldWorkWithLinqAndDynamicsAsync()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				dynamic cars = new ExpandoObject();
				cars.Description = "Cars";

				dynamic monaro = new ExpandoObject();
				monaro.ProductLine = cars;
				monaro.Name = "Monaro";
				monaro.Description = "Holden Monaro";

				dynamic hsv = new ExpandoObject();
				hsv.ProductLine = cars;
				hsv.Name = "hsv";
				hsv.Description = "Holden hsv";

				var models = new List<dynamic> {monaro, hsv};

				cars.Models = models;

				await (s.SaveAsync("ProductLine", cars));
				await (t.CommitAsync());
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var cars = await (s.Query<dynamic>("ProductLine").OrderBy("Description").SingleAsync());
				var models = cars.Models;
				Assert.That(NHibernateUtil.IsInitialized(models), Is.False);
				Assert.That(models.Count, Is.EqualTo(2));
				Assert.That(NHibernateUtil.IsInitialized(models), Is.True);
				s.Clear();

				var list = await (s.Query<dynamic>("Model").Where("ProductLine.Description = @0", "Cars").ToListAsync());
				foreach (var model in list)
				{
					Assert.That(NHibernateUtil.IsInitialized(model.ProductLine), Is.False);
				}
				var model1 = list[0];
				Assert.That(model1.ProductLine.Models.Contains(model1), Is.True);
				s.Clear();

				await (t.CommitAsync());
			}
		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Delete("from ProductLine");
				t.Commit();
			}
		}
	}
}
