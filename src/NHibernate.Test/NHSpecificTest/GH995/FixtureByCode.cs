using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH995
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private Guid _parentId;
		private Guid _childId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Guid));
					rc.Discriminator(x => x.Column("Type"));
				});

			mapper.Subclass<Parent1>(rc => rc.DiscriminatorValue("Parent1"));
			mapper.Subclass<Parent2>(rc => rc.DiscriminatorValue("Parent2"));

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Guid));
					rc.Property(x => x.ParentId, m => m.Column("ParentId"));
					rc.ManyToOne(
						x => x.Parent,
						m =>
						{
							m.Column("ParentId");
							m.Class(typeof(Parent));
							m.Access(Accessor.None);
							m.Insert(false);
							m.Update(false);
							m.NotNullable(false);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parent = new Parent2();
			session.Save(parent);

			var child = new Child { ParentId = parent.Id };
			session.Save(child);

			transaction.Commit();

			_parentId = parent.Id;
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
		public void CanLoadSubclassAfterLoadingItsIdThroughAccessNoneProperty()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Loading the child resolves its access="none" many-to-one internally,
			// which must not prevent the subsequent Get from yielding the actual Parent2 instance.
			var child = session.Get<Child>(_childId);
			var parent = session.Get<Parent>(child.ParentId.Value);

			Assert.That(parent, Is.InstanceOf<Parent2>());
		}
	}
}
