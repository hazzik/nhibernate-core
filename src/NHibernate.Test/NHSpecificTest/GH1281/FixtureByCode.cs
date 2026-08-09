using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1281
{
	// NH-3204
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Customer>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name);
					rc.Bag(
						x => x.Addresses,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(k => k.Column("CustomerId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<Address>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Street);
					rc.ManyToOne(x => x.Customer, m => m.Column("CustomerId"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Customer 1 has multiple addresses, so its joined rows alone fill up the
			// requested page size, "starving" the following customers.
			var customer1 = new Customer { Id = 1, Name = "Customer1" };
			customer1.Addresses.Add(new Address { Id = 1, Street = "Street1a", Customer = customer1 });
			customer1.Addresses.Add(new Address { Id = 2, Street = "Street1b", Customer = customer1 });
			customer1.Addresses.Add(new Address { Id = 3, Street = "Street1c", Customer = customer1 });

			var customer2 = new Customer { Id = 2, Name = "Customer2" };
			customer2.Addresses.Add(new Address { Id = 4, Street = "Street2a", Customer = customer2 });

			var customer3 = new Customer { Id = 3, Name = "Customer3" };
			customer3.Addresses.Add(new Address { Id = 5, Street = "Street3a", Customer = customer3 });

			session.Save(customer1);
			session.Save(customer2);
			session.Save(customer3);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Address").ExecuteUpdate();
			session.CreateQuery("delete from Customer").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void FetchWithTakeReturnsRequestedNumberOfRootEntities()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var customers = session.Query<Customer>()
				.OrderBy(c => c.Id)
				.Fetch(c => c.Addresses)
				.Take(2)
				.ToList();

			// Paging must apply to the number of root (Customer) entities, not to the
			// number of joined rows produced by the collection fetch.
			Assert.That(customers, Has.Count.EqualTo(2));
			Assert.That(customers.Select(c => c.Name), Is.EquivalentTo(new[] { "Customer1", "Customer2" }));
		}
	}
}
