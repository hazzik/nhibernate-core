using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1240
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
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Price);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new Product { Id = 1, Price = 1.5 });
				transaction.Commit();
			}

			// Simulate a database with a null cell in a column mapped to a non-nullable value type property.
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateSQLQuery("update Product set Price = null where Id = 1").ExecuteUpdate();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from Product").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void SelectingNonNullableValueTypePropertyWithNullDatabaseValueDoesNotThrow()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				ProductDto dto = null;
				Assert.DoesNotThrow(
					() => dto = session.Query<Product>().Select(p => new ProductDto { Id = p.Id, Price = p.Price }).Single());

				Assert.That(dto.Price, Is.EqualTo(0d));
			}
		}
	}
}
