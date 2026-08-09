using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1350
{
	// NH-3894 - Keys with custom GetHashCode are not added correctly to key-many-to-many/one-to-many
	// dictionaries when FetchMode is not Select.
	//
	// Container.Items is a one-to-many map indexed by a key-many-to-many (Tag), reusing the very same column
	// as Element's own "Tag" many-to-one association (a ternary relationship). Tag is mapped non-lazy, so its
	// many-to-one association from Element defaults to an eager outer join. When the collection is loaded, the
	// loader's join walker (OneToManyJoinWalker) instantiates a hollow Tag (identifier only) as part of joining
	// Element.Tag, registers it in the persistence context, and only hydrates its state afterwards. Meanwhile,
	// the map's key-many-to-many resolves the very same row to that same, still hollow, Tag instance and uses
	// it right away as the dictionary key. Since Tag has a custom GetHashCode based on Name, every key ends up
	// hashing/comparing as if Name were null at the moment it is inserted, so entries collapse onto each other.
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Tag>(
				rc =>
				{
					rc.Lazy(false);
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
				});

			mapper.Class<Element>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.ManyToOne(x => x.Tag, m => m.Column("TagId"));
					rc.Property(x => x.Value);
				});

			mapper.Class<Container>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Map(
						x => x.Items,
						map =>
						{
							map.Key(k => k.Column("ContainerId"));
							map.Cascade(Mapping.ByCode.Cascade.All);
						},
						key => key.ManyToMany(m => m.Column("TagId")),
						element => element.OneToMany());
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Delete in FK order: Element references both Container and Tag.
			session.CreateQuery("delete from Element").ExecuteUpdate();
			session.CreateQuery("delete from Container").ExecuteUpdate();
			session.CreateQuery("delete from Tag").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void KeysWithCustomGetHashCodeAreAddedCorrectly()
		{
			int containerId;
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var tagA = new Tag { Name = "A" };
				var tagB = new Tag { Name = "B" };
				var tagC = new Tag { Name = "C" };
				session.Save(tagA);
				session.Save(tagB);
				session.Save(tagC);

				var container = new Container();
				container.Items[tagA] = new Element { Tag = tagA, Value = "one" };
				container.Items[tagB] = new Element { Tag = tagB, Value = "two" };
				container.Items[tagC] = new Element { Tag = tagC, Value = "three" };
				containerId = (int) session.Save(container);

				transaction.Commit();
			}

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var container = session.Get<Container>(containerId);

				Assert.That(
					container.Items,
					Has.Count.EqualTo(3),
					"Keys were collapsed into fewer dictionary entries because they were hashed before being fully hydrated.");
				Assert.That(
					container.Items.Values.Select(e => e.Value).OrderBy(v => v),
					Is.EqualTo(new[] { "one", "three", "two" }));

				transaction.Commit();
			}
		}
	}
}
