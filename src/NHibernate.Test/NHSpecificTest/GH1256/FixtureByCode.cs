using System.Collections;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1256
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Cfg.Environment.GenerateStatistics, "true");
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<AbstractParent>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Name);
			});

			mapper.JoinedSubclass<Parent>(rc =>
			{
				rc.Bag(
					x => x.Children,
					m =>
					{
						m.Key(k => k.Column("ParentId"));
						m.Cascade(Mapping.ByCode.Cascade.All);
						m.Fetch(CollectionFetchMode.Subselect);
					},
					r => r.OneToMany());
			});

			mapper.Class<Child>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Name);
				rc.ManyToOne(x => x.Parent, m => m.Column("ParentId"));
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var p1 = new Parent {Name = "foo"};
				p1.Children.Add(new Child {Name = "foo1", Parent = p1});
				p1.Children.Add(new Child {Name = "foo2", Parent = p1});

				var p2 = new Parent {Name = "bar"};
				p2.Children.Add(new Child {Name = "bar1", Parent = p2});
				p2.Children.Add(new Child {Name = "bar2", Parent = p2});

				session.Save(p1);
				session.Save(p2);

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from Child").ExecuteUpdate();
				session.CreateQuery("delete from AbstractParent").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void SubselectFetchWorksWhenParentLoadedByQueryingBaseClass()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				Sfi.Statistics.Clear();

				// Query the base class, not the subclass declaring the subselect-fetched collection.
				var parents = session.CreateQuery("from AbstractParent order by Name desc").List();

				var p1 = (Parent) parents[0];
				var p2 = (Parent) parents[1];

				Assert.That(NHibernateUtil.IsInitialized(p1.Children), Is.False);
				Assert.That(NHibernateUtil.IsInitialized(p2.Children), Is.False);

				// Touching the first parent's collection should trigger a single subselect
				// that also initializes the second parent's collection.
				Assert.That(p1.Children, Has.Count.EqualTo(2));

				Assert.That(
					NHibernateUtil.IsInitialized(p2.Children),
					Is.True,
					"Second parent's collection should have been initialized by the subselect fetch triggered by the first.");

				// One select for the parents, one subselect for all children: no N+1.
				Assert.That(Sfi.Statistics.PrepareStatementCount, Is.EqualTo(2));

				transaction.Commit();
			}
		}
	}
}
