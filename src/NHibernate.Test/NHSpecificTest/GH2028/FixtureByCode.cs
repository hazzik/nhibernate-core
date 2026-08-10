using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2028
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.ParentName);
					rc.Set(
						x => x.Childs,
						m =>
						{
							m.Cascade(Mapping.ByCode.Cascade.All | Mapping.ByCode.Cascade.DeleteOrphans);
							m.Key(k => k.Column("parent_id"));
						},
						r => r.OneToMany());
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.ChildName);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Parent { Id = "1", ParentName = "Parent 1" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Child").ExecuteUpdate();
			session.CreateQuery("delete from Parent").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void RemovingChildAfterAutoFlushDoesNotInsertOrphan()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var parent = session.Get<Parent>("1");

				var child = new Child { Id = "99", ChildName = "Child 99" };
				parent.Childs.Add(child);

				// Triggers an auto-flush, which inserts the still-referenced child.
				session.CreateQuery("from Parent").List();

				// The child is no longer referenced by the collection when the transaction commits.
				parent.Childs.Remove(child);

				session.Update(parent);
				transaction.Commit();
			}

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var child = session.Get<Child>("99");

				Assert.That(child, Is.Null, "Child removed from the collection before commit should not remain as an orphan row.");

				transaction.Commit();
			}
		}
	}
}
