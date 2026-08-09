using System;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1266
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Client>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
				});

			mapper.Class<Purchase>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.ManyToOne(x => x.Client);
					rc.Property(x => x.Date);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var client1 = new Client { Name = "Client1" };
			var client2 = new Client { Name = "Client2" };
			session.Save(client1);
			session.Save(client2);

			session.Save(new Purchase { Client = client1, Date = new DateTime(2020, 1, 1) });
			session.Save(new Purchase { Client = client1, Date = new DateTime(2020, 1, 5) });
			session.Save(new Purchase { Client = client2, Date = new DateTime(2020, 1, 3) });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Purchase").ExecuteUpdate();
			session.CreateQuery("delete from Client").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void SelectingFirstOfEachGroupReturnsMostRecentPurchasePerClient()
		{
			using var session = OpenSession();

			var orders = session.Query<Purchase>()
				.OrderByDescending(l => l.Date)
				.GroupBy(l => l.Client)
				.Select(l => l.First())
				.ToList();

			Assert.That(orders, Has.Count.EqualTo(2));
			Assert.That(orders.Select(o => o.Date), Is.EquivalentTo(new[] { new DateTime(2020, 1, 5), new DateTime(2020, 1, 3) }));
		}
	}
}
