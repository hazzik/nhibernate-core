using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1040
{
	// NH-3788 (GH-1040): inserting an entity that has a composite id and a many-to-one
	// association whose foreign key columns partially overlap with the id's own columns
	// used to fail.
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Item>(
				rc =>
				{
					rc.ComponentAsId(
						x => x.Id,
						m =>
						{
							m.Property(x => x.PhaseId, cm => cm.Column("PhaseId"));
							m.Property(x => x.Num, cm => cm.Column("Num"));
						});

					rc.Property(x => x.Name);

					rc.ManyToOne(
						x => x.WasCopiedFrom,
						m =>
						{
							m.Columns(c => c.Name("PhaseId"), c => c.Name("CopiedNum"));
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Item").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanInsertEntityWithManyToOneSharingColumnWithCompositeId()
		{
			var original = new Item { Id = new ItemId { PhaseId = 1, Num = 1 }, Name = "Original" };
			var copy = new Item { Id = new ItemId { PhaseId = 1, Num = 2 }, Name = "Copy", WasCopiedFrom = original };

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(original);
				session.Save(copy);
				transaction.Commit();
			}

			using (var session = OpenSession())
			{
				var reloaded = session.Get<Item>(new ItemId { PhaseId = 1, Num = 2 });

				Assert.That(reloaded, Is.Not.Null);
				Assert.That(reloaded.WasCopiedFrom, Is.Not.Null, "CopiedNum column was not inserted correctly");
				Assert.That(reloaded.WasCopiedFrom.Id.Num, Is.EqualTo(1));
			}
		}
	}
}
