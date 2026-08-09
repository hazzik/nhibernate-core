using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1307
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Shipper>(
				rc =>
				{
					rc.Id(x => x.ShipperId, m => m.Generator(Generators.Native));
					rc.Property(x => x.CompanyName);
					rc.Bag(
						x => x.Orders,
						m =>
						{
							m.Key(k => k.Column("ShipperId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());
				});

			mapper.Class<Order>(
				rc =>
				{
					rc.Table("`Order`");
					rc.Id(x => x.OrderId, m => m.Generator(Generators.Native));
					rc.ManyToOne(x => x.Shipper, m => m.Column("ShipperId"));
					rc.Bag(
						x => x.OrderLines,
						m =>
						{
							m.Key(k => k.Column("OrderId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());
				});

			mapper.Class<OrderLine>(
				rc =>
				{
					rc.Id(x => x.OrderLineId, m => m.Generator(Generators.Native));
					rc.ManyToOne(x => x.Order, m => m.Column("OrderId"));
					rc.Property(x => x.ProductName);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		private int _shipperId;

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var shipper = new Shipper { CompanyName = "Acme Shipping" };

			var order = new Order { Shipper = shipper };
			shipper.Orders.Add(order);

			order.OrderLines.Add(new OrderLine { Order = order, ProductName = "Widget" });
			order.OrderLines.Add(new OrderLine { Order = order, ProductName = "Gadget" });

			session.Save(shipper);

			transaction.Commit();

			_shipperId = shipper.ShipperId;
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from OrderLine").ExecuteUpdate();
			session.CreateQuery("delete from Order").ExecuteUpdate();
			session.CreateQuery("delete from Shipper").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ProjectNestedSubcollections()
		{
			using var session = OpenSession();

			var query = from s in session.Query<Shipper>()
						where s.ShipperId == _shipperId
						select new { Name = s.CompanyName, Orders = s.Orders.Select(o => new { o.OrderLines }) };

			var result = query.ToList();

			Assert.That(result, Has.Count.EqualTo(1));

			var orders = result[0].Orders.ToList();
			Assert.That(orders, Has.Count.EqualTo(1));
			Assert.That(orders[0].OrderLines, Is.Not.Null);
			Assert.That(orders[0].OrderLines.Count(), Is.EqualTo(2));
		}
	}
}
