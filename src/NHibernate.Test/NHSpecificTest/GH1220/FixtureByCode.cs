using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1220
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Product>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
					rc.Map(
						x => x.CountryVariations,
						m => { },
						key => key.ManyToMany(k => k.Column("CountryId")),
						element => element.ManyToMany(e => e.Column("VariationId")));
				});

			mapper.Class<Country>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
				});

			mapper.Class<ProductCountryVariation>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.LocalName);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var country = new Country { Name = "Denmark" };
			var variation = new ProductCountryVariation { LocalName = "Danish product" };
			session.Save(country);
			session.Save(variation);

			var product = new Product { Name = "Widget" };
			product.CountryVariations.Add(country, variation);
			session.Save(product);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Clear the many-to-many join table first, bulk HQL deletes below do not cascade to it.
			foreach (var product in session.Query<Product>().ToList())
			{
				product.CountryVariations.Clear();
			}

			session.Flush();

			session.CreateQuery("delete from System.Object").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanQueryAnyOnDictionaryKeyProperty()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var countryId = session.Query<Country>().Select(c => c.Id).Single();

			var products = session
				.Query<Product>()
				.Where(p => p.CountryVariations.Any(kv => kv.Key.Id == countryId))
				.ToList();

			Assert.That(products, Has.Count.EqualTo(1));

			transaction.Commit();
		}
	}
}
