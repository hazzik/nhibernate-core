using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1036
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
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
				});

			mapper.Class<OrderLine>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.ManyToOne(x => x.Product);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var product = new Product { Name = "Widget" };
			session.Save(product);
			session.Save(new OrderLine { Product = product });
			session.Save(new OrderLine { Product = product });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from OrderLine").ExecuteUpdate();
			session.CreateQuery("delete from Product").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanSelectAfterDistinct()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var products = session.Query<OrderLine>().Distinct().Select(x => x.Product).ToList();

			Assert.That(products, Has.Count.EqualTo(2));
		}
	}
}
