using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1031
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private int _id;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Product>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
					rc.Component(
						x => x.Image,
						m =>
						{
							m.Property(x => x.Caption);
							m.Property(x => x.Content, pm => pm.Lazy(true));
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var product = new Product
			{
				Name = "Widget",
				Image = new Image {Caption = "Logo", Content = "binary-data"}
			};
			session.Save(product);
			_id = product.Id;

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
		public void LazyPropertyInsideComponentIsNotFetchedEagerly()
		{
			using var session = OpenSession();
			using var log = new SqlLogSpy();

			session.Get<Product>(_id);

			Assert.That(log.GetWholeLog(), Does.Not.Contain("Content"),
				"The lazy property inside the component should not be selected by the initial load.");
		}
	}
}
