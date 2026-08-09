using System;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1982
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Order>(
				rc =>
				{
					rc.Table("`Order`");
					rc.Id(x => x.OrderId, m => m.Generator(Generators.Native));
					rc.Property(x => x.OrderDate);
				});

			mapper.Class<OrderLine>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.ManyToOne(x => x.Order, m => m.Column("OrderId"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var order1 = new Order { OrderDate = new DateTime(2020, 1, 1) };
			session.Save(order1);
			var order2 = new Order { OrderDate = new DateTime(2020, 2, 1) };
			session.Save(order2);

			session.Save(new OrderLine { Order = order1 });
			session.Save(new OrderLine { Order = order1 });
			session.Save(new OrderLine { Order = order2 });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from OrderLine").ExecuteUpdate();
			session.CreateQuery("delete from System.Object").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void GroupByJoinedKeyWithToStringProjectionUsesConsistentTableReference()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var result = session.Query<OrderLine>()
				.GroupBy(x => new { x.Order.OrderId, x.Order.OrderDate })
				.Select(x => new { OrderIdString = x.Key.OrderId.ToString(), x.Key.OrderDate, Count = x.Count() })
				.OrderBy(x => x.OrderIdString)
				.ToArray();

			transaction.Commit();

			Assert.That(result, Has.Length.EqualTo(2));
			Assert.That(result[0].OrderIdString, Is.EqualTo("1"));
			Assert.That(result[0].Count, Is.EqualTo(2));
			Assert.That(result[1].OrderIdString, Is.EqualTo("2"));
			Assert.That(result[1].Count, Is.EqualTo(1));
		}
	}
}
