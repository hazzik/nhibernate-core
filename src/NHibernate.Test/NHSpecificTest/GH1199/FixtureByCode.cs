using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1199
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Kpi>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Set(
						x => x.Columns,
						m =>
						{
							m.Key(k => k.Column("KpiId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());
				});

			mapper.Class<KpiColumn>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.ManyToOne(x => x.Kpi, m => m.Column("KpiId"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var kpiWithColumns = new Kpi { Id = 1 };
			kpiWithColumns.Columns.Add(new KpiColumn { Id = 1, Kpi = kpiWithColumns });
			kpiWithColumns.Columns.Add(new KpiColumn { Id = 2, Kpi = kpiWithColumns });
			session.Save(kpiWithColumns);

			var kpiWithoutColumns = new Kpi { Id = 2 };
			session.Save(kpiWithoutColumns);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from KpiColumn").ExecuteUpdate();
			session.CreateQuery("delete from Kpi").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void LeftJoinFetchInitializesEmptyCollectionsToo()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Load the entities into the session first, mimicking the ":kpis" parameter from the report.
			var kpis = session.Query<Kpi>().OrderBy(k => k.Id).ToList();

			session
				.CreateQuery("select k.Id from Kpi k left join fetch k.Columns where k in (:kpis)")
				.SetParameterList("kpis", kpis)
				.List();

			var kpiWithColumns = kpis.Single(k => k.Id == 1);
			var kpiWithoutColumns = kpis.Single(k => k.Id == 2);

			Assert.That(NHibernateUtil.IsInitialized(kpiWithColumns.Columns), Is.True, "Collection of the Kpi with a non-empty collection should be initialized.");
			Assert.That(NHibernateUtil.IsInitialized(kpiWithoutColumns.Columns), Is.True, "Collection of the Kpi with an empty collection should be initialized too.");

			transaction.Commit();
		}
	}
}
