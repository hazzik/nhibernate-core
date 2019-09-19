using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3576
{
	[TestFixture]
	public class FilterTest : BugTestCase
	{
		private object tenantId;
		private object pricelistId;

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			{
				var tenant = new Tenant
				{
					Name = "gaudi",
					BaseUrl = "http://localhost:8061/gaudi",
					Theme = "gaudi",
					Title = "gaudi"
				};
				tenantId = session.Save(tenant);

				var pricelist = new Pricelist
				{
					Tenant = tenant,
					Name = "sales",
					Precision = 2,
					IsActive = true
				};
				pricelistId = session.Save(pricelist);

				session.Flush();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			{
				session.Delete("from Pricelist");
				session.Delete("from Tenant");
				session.Flush();
			}
		}

		[Test]
		public void ShouldBeLoadedWithoutException()
		{
			using (var session = OpenSession())
			{
				session.EnableFilter("tenant").SetParameter("id", tenantId);
				session.Load<Pricelist>(pricelistId);
			}
		}
	}
}
