using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1020
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
					rc.Property(x => x.Name);
					rc.Set(
						x => x.Children,
						m =>
						{
							m.Key(k => k.Column("ParentId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
							m.Fetch(CollectionFetchMode.Select);
						},
						r => r.OneToMany());
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.ComponentAsId(
						x => x.Id,
						c =>
						{
							c.ManyToOne(x => x.Parent, m => m.Column("ParentId"));
							c.Property(x => x.Sequence);
						});
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parent = new Parent { Id = 1, Name = "Parent1" };
			var child1 = new Child { Id = new ChildId { Parent = parent, Sequence = 1 }, Name = "Child1" };
			var child2 = new Child { Id = new ChildId { Parent = parent, Sequence = 2 }, Name = "Child2" };
			parent.Children.Add(child1);
			parent.Children.Add(child2);

			session.Save(parent);

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
		public void RefreshDoesNotCreateDuplicateInstances()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parent = session.Get<Parent>(1);
			var childBefore = parent.Children.OrderBy(c => c.Id.Sequence).First();

			session.Refresh(parent);

			Assert.That(NHibernateUtil.IsInitialized(parent), Is.True,
				"The refreshed parent should still be the same, fully initialized instance, not an uninitialized proxy.");

			var childAfter = parent.Children.OrderBy(c => c.Id.Sequence).First();
			Assert.That(childAfter, Is.SameAs(childBefore),
				"Refresh must not create a duplicate instance for the same child row in the session cache.");

			var parentAgain = session.Get<Parent>(1);
			Assert.That(parentAgain, Is.SameAs(parent),
				"Session.Get after Refresh should return the same object instance that was refreshed, not a new proxy.");

			transaction.Commit();
		}
	}
}
