using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1054
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
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
					rc.ManyToOne(
						x => x.Parent,
						m =>
						{
							m.Column("ParentId");
							m.Lazy(LazyRelation.NoProxy);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		private int _childId;

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parent = new Parent { Name = "original parent" };
			session.Save(parent);

			var child = new Child { Name = "original child", Parent = parent };
			session.Save(child);

			transaction.Commit();

			_childId = child.Id;
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
		public void CanUpdateEntityFetchedByStatelessGetWhenAssociationIsNoProxy()
		{
			using var statelessSession = Sfi.OpenStatelessSession();
			using var transaction = statelessSession.BeginTransaction();

			var child = statelessSession.Get<Child>(_childId);
			child.Name = "updated child";

			// This should simply issue an UPDATE for the Child row. It must not require
			// resolving the no-proxy Parent association, since a stateless session cannot
			// initialize lazy associations after the fact.
			statelessSession.Update(child);

			transaction.Commit();

			using var verifySession = Sfi.OpenStatelessSession();
			var reloaded = verifySession.Get<Child>(_childId);
			Assert.That(reloaded.Name, Is.EqualTo("updated child"));
		}
	}
}
