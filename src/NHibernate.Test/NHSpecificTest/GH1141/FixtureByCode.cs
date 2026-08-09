using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1141
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Purchase>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Number);
					rc.Bag(
						x => x.Items,
						m => m.Cascade(Mapping.ByCode.Cascade.All),
						r => r.OneToMany());
				});

			mapper.Class<Item>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var order = new Purchase { Number = "123" };
			order.Items.Add(new Item { Name = "Item1" });
			order.Items.Add(new Item { Name = "Item2" });
			order.Items.Add(new Item { Name = "Item3" });
			session.Save(order);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Item").ExecuteUpdate();
			session.CreateQuery("delete from Purchase").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void FirstOrDefaultWithFetchReturnsAllChildren()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var order = session
				.Query<Purchase>()
				.Where(o => o.Number == "123")
				.Fetch(o => o.Items)
				.FirstOrDefault();

			Assert.That(order, Is.Not.Null);
			Assert.That(order.Items.Count, Is.EqualTo(3), "FirstOrDefault combined with Fetch on a collection must not truncate the fetched collection");

			transaction.Commit();
		}
	}
}
