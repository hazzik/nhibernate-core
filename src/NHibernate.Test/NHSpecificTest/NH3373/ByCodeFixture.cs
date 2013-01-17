using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3373
{
	public class ByCodeFixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Company>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);
					rc.Component(x => x.Address);
				});
			mapper.Class<Country>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);
				});
			mapper.Component<AddressComponent>(
				rc =>
				{
					rc.Property(x => x.Street);
					rc.ManyToOne(y => y.Country);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var c1 = new Country {Name = "Austria"};
				session.Save(c1);

				var e1 = new Company
				{
					Name = "Bob",
					Address = new AddressComponent {Country = c1}
				};
				session.Save(e1);

				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public void LinqFetchThroughComponent_EntityOnComponentEagerLoaded()
		{
			Company company;

			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				company =
					session.Query<Company>()
					       .Fetch(x => x.Address).ThenFetch(x => x.Country)
					       .SingleOrDefault();
			}

			Assert.IsTrue(
				NHibernateUtil.IsInitialized(company.Address.Country),
				"Country entity on Component 'AddressComponent' not loaded!");
		}
		
		[Test]
		public void HqlFetchThroughComponent_EntityOnComponentEagerLoaded()
		{
			Company company;

			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				company =
					session
						.CreateQuery("select c from Company c join fetch c.Address.Country")
						.SetResultTransformer(Transformers.DistinctRootEntity)
						.UniqueResult<Company>();
			}

			Assert.IsTrue(
				NHibernateUtil.IsInitialized(company.Address.Country),
				"Country entity on Component 'AddressComponent' not loaded!");
		}
	}
}