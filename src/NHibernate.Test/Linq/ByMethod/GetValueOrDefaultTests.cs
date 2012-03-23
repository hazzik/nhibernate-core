using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg;
using NHibernate.DomainModel.Northwind.Entities;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.Linq.ByMethod
{
	public class GetValueOrDefaultTests : LinqTestCase
	{
		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);
			configuration.SetProperty(Environment.ShowSql, "true");
		}

		[Test]
		public void WhereTest()
		{
			//NH-3096
			List<Product> orders = (from o in db.Products
									where o.UnitPrice.GetValueOrDefault() == 0
									select o).ToList();

			int count = orders.Count;

			Console.WriteLine(count);
		}

		[Test]
		public void Where2Test()
		{
			//NH-3096
			List<Product> orders = (from o in db.Products
									where o.UnitPrice.GetValueOrDefault(1) == 1
									select o).ToList();

			int count = orders.Count;

			Console.WriteLine(count);
		}

		[Test]
		public void Where3Test()
		{
			//NH-3096
			List<Product> orders = (from o in db.Products
									where (o.UnitPrice ?? 1) == 1
									select o).ToList();

			int count = orders.Count;

			Console.WriteLine(count);
		}
	}
}
