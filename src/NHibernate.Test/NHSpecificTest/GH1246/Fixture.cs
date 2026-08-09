using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1246
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
					rc.Join(
						"GH1246EntityData",
						join =>
						{
							join.Key(k => k.Column("EntityId"));
							join.Optional(true);
							join.Component(
								x => x.Data,
								c =>
								{
									c.Property(x => x.Value1);
									c.Property(x => x.Value2);
								});
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Entity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void UpdatingEmptyJoinedComponentToNonEmptyPerformsUpdateNotInsert()
		{
			int id;
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				// The component instance is not null, but all its properties are, so the
				// row inserted into the joined table only contains null data columns.
				var entity = new Entity { Name = "Bob", Data = new JoinedComponent() };
				id = (int) s.Save(entity);
				t.Commit();
			}

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				var entity = s.Get<Entity>(id);

				// Just like outside of a join, an entirely null component is loaded as null.
				Assert.That(entity.Data, Is.Null, "An empty joined component should be loaded as null.");

				entity.Data = new JoinedComponent {Value1 = "foo", Value2 = "bar"};

				// The row for the joined table already exists (with null columns), so this
				// must issue an UPDATE. Issuing an INSERT instead fails, since a row is
				// already present for that key.
				Assert.That(() => t.Commit(), Throws.Nothing);
			}

			using (var s = OpenSession())
			{
				var entity = s.Get<Entity>(id);
				Assert.That(entity.Data, Is.Not.Null);
				Assert.That(entity.Data.Value1, Is.EqualTo("foo"));
				Assert.That(entity.Data.Value2, Is.EqualTo("bar"));
			}
		}
	}
}
