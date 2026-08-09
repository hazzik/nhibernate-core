using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1142
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private int _parentId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

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
						},
						r => r.OneToMany());
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parent = new Parent { Name = "Original" };
			parent.Children.Add(new Child());
			parent.Children.Add(new Child());
			session.Save(parent);

			transaction.Commit();

			_parentId = parent.Id;
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
		public void CanAccessChildrenCollectionFromOnLoadDuringMerge()
		{
			Parent.ObservedChildrenCountOnLoad = -1;

			// A detached entity, as would come back from a previous request/session.
			var detached = new Parent { Id = _parentId, Name = "Updated" };

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Merge(detached);
				transaction.Commit();
			}

			Assert.That(Parent.ObservedChildrenCountOnLoad, Is.EqualTo(2));
		}
	}
}
