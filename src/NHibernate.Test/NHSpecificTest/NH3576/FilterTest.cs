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

		protected override bool AppliesTo(NHibernate.Dialect.Dialect dialect)
		{
			return dialect is MsSql2008Dialect;
		}

		[Test]
		public void ShouldBeLoadedWithoutException()
		{
			using (var session = OpenSession())
			{
				session.EnableFilter("tenant").SetParameter("id", tenantId);
				session.Load<Pricelist>(pricelistId);

				/* EXPECTED:
				 * 
				 SELECT pricelist0_.ID as ID0_2_, pricelist0_.TENANT_ID as TENANT2_0_2_, pricelist0_.BASED_ON as BASED3_0_2_, 
						 pricelist0_.NAME as NAME0_2_, pricelist0_.PRECISION as PRECISION0_2_, pricelist0_.IS_ACTIVE as IS6_0_2_, 
						 tenant1_.ID as ID1_0_, tenant1_.NAME as NAME1_0_, tenant1_.BASE_URL as BASE3_1_0_, tenant1_.TITLE as TITLE1_0_, 
						 tenant1_.THEME as THEME1_0_, pricelist2_.ID as ID0_1_, pricelist2_.TENANT_ID as TENANT2_0_1_, 
						 pricelist2_.BASED_ON as BASED3_0_1_, pricelist2_.NAME as NAME0_1_, pricelist2_.PRECISION as PRECISION0_1_, pricelist2_.IS_ACTIVE as IS6_0_1_ 
				  FROM Pricelists pricelist0_ 
				  inner join Tenants tenant1_ on pricelist0_.TENANT_ID=tenant1_.ID 
				  left outer join Pricelists pricelist2_ on pricelist0_.BASED_ON=pricelist2_.ID and pricelist2_.TENANT_ID = @p0
				  WHERE tenant1_.ID = @p0 AND pricelist0_.ID=@p2;@p0 = 1 [Type: Int64 (0)], @p2 = 1 [Type: Int64 (0)]
				*/

				/* BUT:
				 * 
				  SELECT pricelist0_.ID as ID0_2_, pricelist0_.TENANT_ID as TENANT2_0_2_, pricelist0_.BASED_ON as BASED3_0_2_, 
						 pricelist0_.NAME as NAME0_2_, pricelist0_.PRECISION as PRECISION0_2_, pricelist0_.IS_ACTIVE as IS6_0_2_, 
						 tenant1_.ID as ID1_0_, tenant1_.NAME as NAME1_0_, tenant1_.BASE_URL as BASE3_1_0_, tenant1_.TITLE as TITLE1_0_, 
						 tenant1_.THEME as THEME1_0_, pricelist2_.ID as ID0_1_, pricelist2_.TENANT_ID as TENANT2_0_1_, 
						 pricelist2_.BASED_ON as BASED3_0_1_, pricelist2_.NAME as NAME0_1_, pricelist2_.PRECISION as PRECISION0_1_, pricelist2_.IS_ACTIVE as IS6_0_1_ 
				  FROM Pricelists pricelist0_ 
				  inner join Tenants tenant1_ on pricelist0_.TENANT_ID=tenant1_.ID 
				  left outer join Pricelists pricelist2_ on pricelist0_.BASED_ON=pricelist2_.ID
				  WHERE tenant1_.ID = @p0 and pricelist2_.TENANT_ID = @p0 AND pricelist0_.ID=@p2;@p0 = 1 [Type: Int64 (0)], @p2 = 1 [Type: Int64 (0)]
				 */
			}
		}
	}
}
