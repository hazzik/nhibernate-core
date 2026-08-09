using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1346
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Abc>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
					// A batch size greater than 1 makes NHibernate load several Abc entities
					// in a single query (through EntityLoader) whenever one of them needs
					// to be initialized.
					rc.BatchSize(10);
					rc.Bag(
						x => x.ArraySizes,
						m =>
						{
							m.Key(k => k.Column("AbcId"));
							m.Cascade(Mapping.ByCode.Cascade.All | Mapping.ByCode.Cascade.DeleteOrphans);
							// The collection is configured to be fetched with a subselect, so that
							// loading it for several owners at once should result in a single query.
							m.Fetch(CollectionFetchMode.Subselect);
						},
						r => r.OneToMany());
				});

			mapper.Class<ArraySize>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Size);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			for (var i = 0; i < 3; i++)
			{
				var abc = new Abc { Name = "Abc" + i };
				abc.ArraySizes.Add(new ArraySize { Size = i * 10 });
				abc.ArraySizes.Add(new ArraySize { Size = i * 10 + 1 });
				session.Save(abc);
			}

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from ArraySize").ExecuteUpdate();
			session.CreateQuery("delete from Abc").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void SubselectFetchingIsUsedWhenCollectionOwnersAreBatchLoaded()
		{
			int[] ids;
			using (var session = OpenSession())
			{
				ids = session.Query<Abc>().OrderBy(x => x.Id).Select(x => x.Id).ToList().ToArray();
			}

			Assert.That(ids, Has.Length.EqualTo(3));

			using var session2 = OpenSession();

			// Create three uninitialized proxies. Initializing the first one should trigger
			// EntityLoader to batch-load all three Abc rows in a single "id in (...)" query,
			// which should also register a subselect for their ArraySizes collections.
			var abcs = ids.Select(id => session2.Load<Abc>(id)).ToList();
			NHibernateUtil.Initialize(abcs[0]);

			using (var log = new SqlLogSpy())
			{
				foreach (var abc in abcs)
				{
					NHibernateUtil.Initialize(abc.ArraySizes);
				}

				Assert.That(
					log.Appender.GetEvents(),
					Has.Length.EqualTo(1),
					"ArraySizes for the batch-loaded Abc entities should have been loaded with a single subselect query, " +
					"but a separate query was issued for each Abc instead.");
			}

			Assert.That(abcs.SelectMany(a => a.ArraySizes).Count(), Is.EqualTo(6));
		}
	}
}
