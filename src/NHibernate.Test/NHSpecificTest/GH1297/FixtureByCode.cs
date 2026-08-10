using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1297
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			// Animal is the root of a union-subclass hierarchy: it has its own table,
			// and each subclass (Cat) has its own, entirely separate table.
			mapper.Class<Animal>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
				rc.Property(x => x.Name);
			});

			mapper.UnionSubclass<Cat>(rc =>
			{
				rc.Property(x => x.NumberOfLegs);
			});

			mapper.Class<Zoo>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
				rc.Property(x => x.Name);
				rc.Bag(
					x => x.Animals,
					m =>
					{
						m.Key(k => k.Column("ZooId"));
						m.Cascade(Mapping.ByCode.Cascade.All);
						m.Inverse(false);
					},
					r => r.OneToMany());
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Animal").ExecuteUpdate();
			session.CreateQuery("delete from Zoo").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void SavingOneToManyOfUnionSubclassUpdatesTheSubclassTable()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var cat = new Cat { Id = 1, Name = "Tom", NumberOfLegs = 4 };
				var zoo = new Zoo { Id = 1, Name = "City Zoo" };
				zoo.Animals.Add(cat);

				// Cat is a leaf of the union-subclass hierarchy, so it lives entirely in its
				// own table. The foreign key update issued for the bag entry must target that
				// table (Cat's), not Animal's (the root's) table.
				session.Save(cat);
				session.Save(zoo);

				transaction.Commit();
			}

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var zoo = session.Get<Zoo>(1);

				Assert.That(
					zoo.Animals.Select(a => a.Id),
					Has.Member(1),
					"The cat should be associated with the zoo, but the foreign-key update for the one-to-many collection targeted the wrong (root) table.");

				transaction.Commit();
			}
		}
	}
}
