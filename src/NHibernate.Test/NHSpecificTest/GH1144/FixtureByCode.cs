using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1144
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
					rc.Property(x => x.Category);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			for (var i = 0; i < 10; i++)
			{
				session.Save(new Product { Name = "Name" + i, Category = "Cat " + (i / 3) });
			}

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Product").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void FilterBeforeCastIsNotIgnored()
		{
			using var session = OpenSession();

			// Casting through object and back should be a no-op that does not affect
			// filters applied before or after the casts.
			var castFirstCount =
				session.Query<Product>()
					   .Cast<object>()
					   .Cast<Product>()
					   .Where(p => p.Name == "Name1")
					   .Where(p => p.Category == "Cat 0")
					   .Count();

			var filterFirstCount =
				session.Query<Product>()
					   .Where(p => p.Name == "Name1")
					   .Cast<object>()
					   .Cast<Product>()
					   .Where(p => p.Category == "Cat 0")
					   .Count();

			Assert.That(filterFirstCount, Is.EqualTo(castFirstCount));
			Assert.That(filterFirstCount, Is.EqualTo(1));
		}
	}
}
