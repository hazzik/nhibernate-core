using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3798
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		private object _seed1Id;
		private object _seed2Id;
		private object _seed11Id;
		private object _seed21Id;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
            {
                rc.Table("Entity");
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.ManyToOne(
					x => x.Parent,
					m =>
					{
						m.Column("ParentId");
						m.Lazy(LazyRelation.NoProxy);
					});
			});

			mapper.JoinedSubclass<SubEntity1>(rc => { rc.Table("SubEntity1"); });

			mapper.JoinedSubclass<SubEntity2>(rc => { rc.Table("SubEntity2");});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			var seed1 = new SubEntity1 { Id = Guid.NewGuid() };
			var seed2 = new SubEntity2 { Id = Guid.NewGuid() };
			var seed1_1 = new SubEntity1 { Id = Guid.NewGuid(), Parent = seed1};
			var seed2_1 = new SubEntity2 { Id = Guid.NewGuid(), Parent = seed2};
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				_seed1Id = session.Save(seed1);
				_seed2Id = session.Save(seed2);
				_seed11Id = session.Save(seed1_1);
				_seed21Id = session.Save(seed2_1);
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void ShouldNotThrowException()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var sub1_1 = session.Get<SubEntity1>(_seed11Id);
				var sub2_1 = session.Get<SubEntity2>(_seed21Id);
				
				// Getting the lazy-loaded parents is not even required to produce the issue below
				// These two lines only works using Laziness.False or Laziness.NoProxy (by design)
				// Comment or uncomment these two lines at will
				var sub1 = (SubEntity1)sub1_1.Parent;
				var sub2 = (SubEntity2)sub2_1.Parent;

				// Crash instead of returning null. It seems when the entity is present in
				// the first level cache, there is a check missing to prevent returning entities
				// not of the queried class. It then crashes in the SessionImpl.Get<T>() method.
				// This only occurs when the Lazyload mapping is configured using Laziness.NoProxy.

				//Alex: The apparent problem is that EntityKey matches by RootEntityName.
				//Need to figure out how to make it more narrow but at the same time allow fuzzy matching.
				var r2 = session.Get<SubEntity2>(_seed1Id);
				Assert.That(r2, Is.Null);
			}
		}
	}
}
