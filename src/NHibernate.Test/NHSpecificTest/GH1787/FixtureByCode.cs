using System.Reflection;
using NHibernate.Cache;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Test.CacheTest.Caches;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1787
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.UseSecondLevelCache, "true");
			configuration.SetProperty(Environment.UseQueryCache, "true");
			configuration.SetProperty(Environment.GenerateStatistics, "true");
			configuration.SetProperty(Environment.CacheProvider, typeof(BatchableCacheProvider).AssemblyQualifiedName);
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
					rc.Bag(
						x => x.Children,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(k => k.Column("ParentId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());

					rc.Cache(
						cm =>
						{
							cm.Include(CacheInclude.All);
							cm.Usage(CacheUsage.ReadWrite);
						});
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.ManyToOne(x => x.Parent, m => m.Column("ParentId"));

					rc.Cache(
						cm =>
						{
							cm.Include(CacheInclude.All);
							cm.Usage(CacheUsage.ReadWrite);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Child").ExecuteUpdate();
			session.CreateQuery("delete from Parent").ExecuteUpdate();

			transaction.Commit();
		}

		// Reported issue: saving entities using an identity generator causes the query spaces of the
		// affected (cached) entities to be pre-invalidated in the timestamps cache many more times than
		// necessary: twice per identity save (once for the ActionQueue.ExecuteInserts() flush done before
		// the cascades run, once for the immediate ActionQueue.Execute(insert) of the identity insert
		// itself), and again for every cascaded save. The set of already pre-invalidated spaces is never
		// tracked, so the same spaces get pre-invalidated over and over as more entities are saved in the
		// same transaction.
		[Test]
		public void SavingEntitiesWithIdentityDoesNotFloodTimestampsCache()
		{
			var timestampsCacheField = typeof(UpdateTimestampsCache).GetField(
				"_updateTimestamps",
				BindingFlags.NonPublic | BindingFlags.Instance);
			Assert.That(timestampsCacheField, Is.Not.Null, "Unable to find _updateTimestamps field");
			var cache = (BatchableCache) timestampsCacheField.GetValue(Sfi.UpdateTimestampsCache);
			Assert.That(cache, Is.Not.Null, "_updateTimestamps is null");

			cache.Clear();
			cache.ClearStatistics();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				const int totalParents = 5;
				const int childrenPerParent = 5;
				for (var i = 0; i < totalParents; i++)
				{
					var parent = new Parent { Name = $"Parent{i}" };
					for (var j = 0; j < childrenPerParent; j++)
					{
						parent.Children.Add(new Child { Parent = parent });
					}

					// Identity generator forces an immediate insert for the parent and each cascaded child.
					s.Save(parent);
				}

				t.Commit();
			}

			// Only two query spaces are ever touched (Parent and Child), so the whole transaction should
			// need at most one pre-invalidation of the accumulated spaces plus one final invalidation on
			// commit, regardless of how many entities were saved.
			Assert.That(cache.PutMultipleCalls, Has.Count.LessThanOrEqualTo(2), "Unexpected number of timestamps cache put-many calls");
		}
	}
}
