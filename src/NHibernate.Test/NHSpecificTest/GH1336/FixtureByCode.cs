using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1336
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Sale>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
				rc.Property(x => x.Category);
				rc.Property(x => x.Amount);
				rc.Property(x => x.Receita);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Sale { Category = "A", Amount = 10m, Receita = 5m });
			session.Save(new Sale { Category = "A", Amount = -10m, Receita = null });
			session.Save(new Sale { Category = "B", Amount = 100m, Receita = 50m });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Sale").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CombiningTwoAggregateConditionsGeneratesHavingClause()
		{
			using var log = new SqlLogSpy();
			using var session = OpenSession();

			var query = session.QueryOver<Sale>()
				.SelectList(list => list
					.SelectGroup(x => x.Category)
					.SelectSum(x => x.Amount))
				.Where(
					Restrictions.Eq(Projections.Sum<Sale>(x => x.Amount), 0m) ||
					Restrictions.IsNull(Projections.Sum<Sale>(x => x.Receita)));

			var results = query.List<object[]>();

			Assert.That(
				log.GetWholeLog(),
				Does.Contain("having").IgnoreCase,
				"The condition combining two aggregate projections should be emitted in a HAVING clause, not a WHERE clause.");

			// Category "A" sums Amount to 0 (10 + -10), so it should be the only group returned.
			Assert.That(results, Has.Count.EqualTo(1));
			Assert.That(results[0][0], Is.EqualTo("A"));
		}
	}
}
