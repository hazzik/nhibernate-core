﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Test.DynamicEntity.Interceptor
{
	using System.Threading.Tasks;
	[TestFixture]
	[Obsolete("Require dynamic proxies")]
	public class InterceptorDynamicEntityAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override string[] Mappings
		{
			get { return new string[] { "DynamicEntity.Interceptor.Customer.hbm.xml" }; }
		}

		protected override void Configure(Configuration configuration)
		{
			configuration.SetInterceptor(new ProxyInterceptor());
		}

		[Test]
		public async Task ItAsync()
		{
			var company = ProxyHelper.NewCompanyProxy();
			var customer = ProxyHelper.NewCustomerProxy();
			// Test saving these dyna-proxies
			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				company.Name = "acme";
				await (session.SaveAsync(company));
				customer.Name = "Steve";
				customer.Company = company;
				await (session.SaveAsync(customer));
				await (tran.CommitAsync());
				session.Close();
			}

			Assert.IsNotNull(company.Id, "company id not assigned");
			Assert.IsNotNull(customer.Id, "customer id not assigned");

			// Test loading these dyna-proxies, along with flush processing
			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				customer = await (session.LoadAsync<Customer>(customer.Id));
				Assert.IsFalse(NHibernateUtil.IsInitialized(customer), "should-be-proxy was initialized");

				customer.Name = "other";
				await (session.FlushAsync());
				Assert.IsFalse(NHibernateUtil.IsInitialized(customer.Company), "should-be-proxy was initialized");

				await (session.RefreshAsync(customer));
				Assert.AreEqual("other", customer.Name, "name not updated");
				Assert.AreEqual("acme", customer.Company.Name, "company association not correct");

				await (tran.CommitAsync());
				session.Close();
			}

			// Test detached entity re-attachment with these dyna-proxies
			customer.Name = "Steve";
			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				await (session.UpdateAsync(customer));
				await (session.FlushAsync());
				await (session.RefreshAsync(customer));
				Assert.AreEqual("Steve", customer.Name, "name not updated");
				await (tran.CommitAsync());
				session.Close();
			}

			// Test querying
			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				int count = (await (session.CreateQuery("from Customer").ListAsync())).Count;
				Assert.AreEqual(1, count, "querying dynamic entity");
				session.Clear();
				count = (await (session.CreateQuery("from Person").ListAsync())).Count;
				Assert.AreEqual(1, count, "querying dynamic entity");
				await (tran.CommitAsync());
				session.Close();
			}

			// test deleteing
			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				await (session.DeleteAsync(company));
				await (session.DeleteAsync(customer));
				await (tran.CommitAsync());
				session.Close();
			}
		}
	}
}
