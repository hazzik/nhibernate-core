using System;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1123
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<OrderEntity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.ShippingDate);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Two orders share the same shipping date, forming one group of size 2.
			session.Save(new OrderEntity { ShippingDate = new DateTime(2020, 1, 1) });
			session.Save(new OrderEntity { ShippingDate = new DateTime(2020, 1, 1) });
			// One order has a distinct shipping date, forming a second group of size 1.
			session.Save(new OrderEntity { ShippingDate = new DateTime(2020, 1, 2) });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from OrderEntity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void GroupByCountCountsTheNumberOfGroups()
		{
			using var session = OpenSession();

			// There are two distinct shipping dates, so counting the groups must yield 2,
			// regardless of how many orders fall into each group.
			var result = session.Query<OrderEntity>()
				.GroupBy(x => x.ShippingDate)
				.Count();

			Assert.That(result, Is.EqualTo(2));
		}
	}
}
