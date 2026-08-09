using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1298
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
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Set(
						x => x.OrderLines,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(k => k.Column("OrderId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<OrderLine>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.ManyToOne(x => x.PurchaseOrder, m => m.Column("OrderId"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var order = new PurchaseOrder();
			order.OrderLines.Add(new OrderLine { PurchaseOrder = order });
			order.OrderLines.Add(new OrderLine { PurchaseOrder = order });
			session.Save(order);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from OrderLine").ExecuteUpdate();
			session.CreateQuery("delete from PurchaseOrder").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void FetchWorksWithCollectionProjection()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var query = from o in session.Query<PurchaseOrder>().FetchMany(x => x.OrderLines)
						select new OrderProjection
						{
							PurchaseOrder = o,
							OrderLines = o.OrderLines
						};

			var result = query.ToList();

			Assert.That(result.Count, Is.EqualTo(1));
			Assert.That(NHibernateUtil.IsInitialized(result[0].PurchaseOrder.OrderLines), Is.True);
			Assert.That(NHibernateUtil.IsInitialized(result[0].OrderLines), Is.True);
			Assert.That(result[0].OrderLines, Is.SameAs(result[0].PurchaseOrder.OrderLines));
			Assert.That(result[0].OrderLines, Has.Count.EqualTo(2));

			transaction.Commit();
		}
	}
}
