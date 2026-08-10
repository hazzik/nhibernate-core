using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH994
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.UseSecondLevelCache, "true");
			configuration.SetProperty(Environment.GenerateStatistics, "true");
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(
				rc =>
				{
					// A generator relying on post-insert (identity) id generation is deliberately avoided:
					// entities inserted through EntityIdentityInsertAction are never put into the second-level
					// cache (a separate, known limitation), which would make it impossible to tell apart from
					// the collection recreate cache-put bug under test here.
					rc.Id(x => x.Id, m => m.Generator(Generators.HighLow));
					rc.Bag(
						x => x.Children,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(k => k.Column("EntityId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Cache(cm => cm.Usage(CacheUsage.ReadWrite));
						},
						r => r.OneToMany());

					rc.Cache(cm => cm.Usage(CacheUsage.ReadWrite));
				});

			mapper.Class<ChildEntity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.HighLow));
					rc.Cache(cm => cm.Usage(CacheUsage.ReadWrite));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from ChildEntity").ExecuteUpdate();
			session.CreateQuery("delete from System.Object").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void RecreatedCollectionIsPutIntoSecondLevelCache()
		{
			var persister = Sfi.GetCollectionPersister(typeof(Entity).FullName + ".Children");
			var regionName = persister.Cache.RegionName;

			Sfi.Statistics.Clear();

			int id;
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = new Entity();
				entity.Children.Add(new ChildEntity());
				entity.Children.Add(new ChildEntity());

				// A brand new, non-inverse collection is persisted through a CollectionRecreateAction
				// rather than a CollectionUpdateAction.
				session.Save(entity);
				transaction.Commit();

				id = entity.Id;
			}

			var cacheStats = Sfi.Statistics.GetSecondLevelCacheStatistics(regionName);
			Assert.That(
				cacheStats.PutCount,
				Is.EqualTo(1),
				"The collection should have been put into the second-level cache after the recreate action commits.");

			Sfi.Statistics.Clear();

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = session.Get<Entity>(id);
				NHibernateUtil.Initialize(entity.Children);
				transaction.Commit();
			}

			cacheStats = Sfi.Statistics.GetSecondLevelCacheStatistics(regionName);
			Assert.That(
				cacheStats.HitCount,
				Is.EqualTo(1),
				"Loading the collection again should have hit the second-level cache instead of the database.");
		}
	}
}
