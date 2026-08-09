using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1252
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private int _parentId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Toy>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
					rc.ManyToOne(x => x.Parent, m => m.Column("ParentId"));
					rc.ManyToOne(
						x => x.DynamicToy,
						m =>
						{
							m.Column("ToyId");
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Access(Accessor.ReadOnly);
						});
				});

			mapper.Class<Parent>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
					rc.Bag(
						x => x.Children,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(k => k.Column("ParentId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
							m.Lazy(CollectionLazy.Lazy);
						},
						r => r.OneToMany());
					rc.ManyToOne(
						x => x.Summary,
						m =>
						{
							m.Column("SummaryId");
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Unique(true);
						});
				});

			mapper.Class<ParentSummary>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.ManyToOne(x => x.Parent, m => m.Column("ParentRefId"));
					rc.ManyToOne(
						x => x.FirstChild,
						m =>
						{
							m.Column("FirstChildId");
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Access(Accessor.ReadOnly);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parent = new Parent { Name = "Parent" };
			var child = new Child { Name = "Child", Parent = parent };
			parent.Children.Add(child);
			parent.Summary = new ParentSummary();

			session.Save(parent);

			transaction.Commit();

			_parentId = parent.Id;
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Parent and ParentSummary reference each other, so the mutual FK must be broken
			// before either can be deleted.
			session.CreateQuery("update Parent set Summary = null").ExecuteUpdate();
			session.CreateQuery("delete from ParentSummary").ExecuteUpdate();
			session.CreateQuery("delete from Child").ExecuteUpdate();
			session.CreateQuery("delete from Parent").ExecuteUpdate();
			session.CreateQuery("delete from Toy").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CascadesToEntityLazilyLoadedDuringFlush()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var parent = session.Get<Parent>(_parentId);

				// Force the Summary proxy to be loaded. Cascading into Summary at flush time will
				// evaluate its FirstChild property, which forces Parent.Children (still lazy at this
				// point) to be loaded from the database, creating a new Child instance whose
				// constructor assigns it a brand new, still-transient Toy.
				parent.Summary.Parent = parent;

				// The cascade of DynamicToy (cascade="all") should still pick up that new Toy and
				// save it, even though the Child that owns it was only discovered mid-flush.
				Assert.DoesNotThrow(() => session.Flush());

				transaction.Commit();
			}
		}
	}
}
