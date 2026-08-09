using System.Collections.Generic;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1230
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Man>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Name);
				rc.ManyToOne(x => x.Owner, m => m.Column("OwnerId"));
			});

			mapper.Class<Owner>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Name);
				rc.Bag(
					x => x.Men,
					m =>
					{
						m.Key(k => k.Column("OwnerId"));
						m.Cascade(Mapping.ByCode.Cascade.All);
						m.Inverse(true);
					},
					r => r.OneToMany());
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var owner = new Owner { Name = "Bob" };
			owner.Men.Add(new Man { Name = "John", Owner = owner });
			owner.Men.Add(new Man { Name = "Jack", Owner = owner });
			session.Save(owner);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Man").ExecuteUpdate();
			session.CreateQuery("delete from Owner").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void EnumerableDoesNotEagerlyInitializeProxiesWithThetaStyleJoin()
		{
			using var statisticsScope = new StatisticsScope(Sfi);
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var query = session.CreateQuery("select m from Man m left join m.Owner");

			// The query already ran and produced a data reader by the time Enumerable<T>() returns,
			// so the statement count captured below only reflects further, unwanted queries caused
			// by merely iterating (without accessing any property) over the already-fetched results.
			var enumerable = query.Enumerable<Man>();

			var men = new List<Man>();
			var statementCountBeforeIteration = Sfi.Statistics.PrepareStatementCount;
			foreach (Man man in enumerable)
			{
				men.Add(man);
			}
			var statementCountAfterIteration = Sfi.Statistics.PrepareStatementCount;

			transaction.Commit();

			Assert.That(men, Has.Count.EqualTo(2));
			Assert.That(
				statementCountAfterIteration,
				Is.EqualTo(statementCountBeforeIteration),
				"Merely iterating the enumerable triggered additional statements, meaning entities were eagerly loaded instead of staying as uninitialized proxies");
			Assert.That(
				men,
				Has.All.Matches<Man>(m => !NHibernateUtil.IsInitialized(m)),
				"Entities were eagerly initialized while merely iterating the enumerable");
		}
	}
}
