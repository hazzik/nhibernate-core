using System.Linq;
using System.Reflection;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Engine.Query;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NHibernate.Util;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3069
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Card>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.CardNo);
					rc.Property(x => x.Dci);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Card { CardNo = "1234", Dci = 1 });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Card").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void SelectClauseWithNotEqualDoesNotPreventQueryPlanCaching()
		{
			// The plan cache is a private implementation detail of QueryPlanCache; it is inspected here
			// via reflection, the same technique used by the GH1547 fixture, because there is no public
			// way to observe whether a query plan got cached.
			var planCacheField = typeof(QueryPlanCache).GetField("planCache", BindingFlags.Instance | BindingFlags.NonPublic);
			var planCache = (SoftLimitMRUCache) planCacheField.GetValue(Sfi.QueryPlanCache);
			planCache.Clear();

			RunQuery();
			RunQuery();

			// A query using != in a projected member-init expression should be cacheable just like one
			// using ==. If the plan is not cached, it gets fully regenerated on every execution instead
			// of being reused, which is the reported performance issue.
			Assert.That(planCache.Count, Is.EqualTo(1), "Query plan should have been cached and reused instead of being regenerated");
		}

		private void RunQuery()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var response = session
				.Query<Card>()
				.Where(x => x.CardNo == "1234")
				.Select(x => new CardResponse { IsLimitExceeded = x.Dci != 2 })
				.SingleOrDefault();

			Assert.That(response, Is.Not.Null);
			Assert.That(response.IsLimitExceeded, Is.True);

			transaction.Commit();
		}
	}
}
