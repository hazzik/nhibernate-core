using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1185
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<PurchaseOrder>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Bag(
						x => x.Lines,
						m =>
						{
							m.Key(k => k.Column("OrderId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<PurchaseOrderLine>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.ManyToOne(x => x.Product, m => m.Fetch(FetchKind.Select));
				});

			mapper.Class<Product>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name);
					// No proxy for Product: this is required for FetchMode.Default to differ from
					// an explicit FetchMode.Select when the association is fetched while lazily
					// initializing a collection of the owning entity.
					rc.Lazy(false);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var product = new Product { Id = 1, Name = "Widget" };
			var line = new PurchaseOrderLine { Id = 1, Product = product };
			var order = new PurchaseOrder { Id = 1 };
			order.Lines.Add(line);

			session.Save(product);
			session.Save(order);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from PurchaseOrderLine").ExecuteUpdate();
			session.CreateQuery("delete from PurchaseOrder").ExecuteUpdate();
			session.CreateQuery("delete from Product").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ExplicitSelectFetchIsHonoredWhenLazilyInitializingOwningCollection()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var order = session.Get<PurchaseOrder>(1);

			using var spy = new SqlLogSpy();
			// Triggers lazy initialization of the Lines collection.
			Assert.That(order.Lines.Count, Is.EqualTo(1));

			// The many-to-one to Product was explicitly mapped with FetchKind.Select, so loading
			// the collection must not outer-join Product: it must be fetched by a separate select
			// on later access.
			Assert.That(spy.GetWholeLog(), Does.Not.Contain("Product").IgnoreCase);

			transaction.Commit();
		}
	}
}
