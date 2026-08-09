using System.Linq;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1248
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Basket>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Bag(
						x => x.Apples,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(k => k.Column("BasketId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());
					rc.Bag(
						x => x.Oranges,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(k => k.Column("BasketId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());
				});

			mapper.Class<Fruit>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Discriminator(d => d.Column("FruitType"));
					rc.ManyToOne(x => x.Basket, m => m.Column("BasketId"));
				});

			mapper.Subclass<Apple>(rc => rc.DiscriminatorValue("Apple"));
			mapper.Subclass<Orange>(rc => rc.DiscriminatorValue("Orange"));

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var basket = new Basket();
			basket.Apples.Add(new Apple { Basket = basket });
			basket.Oranges.Add(new Orange { Basket = basket });
			session.Save(basket);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Fruit").ExecuteUpdate();
			session.CreateQuery("delete from Basket").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void AppleCollectionDoesNotContainOranges()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var basket = session.Query<Basket>().Single();

			Assert.That(basket.Apples, Has.Count.EqualTo(1), "Apples collection should only contain the single Apple");
			Assert.That(basket.Apples.Single(), Is.InstanceOf<Apple>());

			Assert.That(basket.Oranges, Has.Count.EqualTo(1), "Oranges collection should only contain the single Orange");
			Assert.That(basket.Oranges.Single(), Is.InstanceOf<Orange>());

			transaction.Commit();
		}
	}
}
