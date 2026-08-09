using System.Collections;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1167
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private const int CountPerClass = 10;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<EntityA>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
				rc.Property(x => x.Name);
			});

			mapper.Class<EntityB>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
				rc.Property(x => x.Name);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				for (var i = 0; i < CountPerClass; i++)
				{
					session.Save(new EntityA { Name = "A" + i });
					session.Save(new EntityB { Name = "B" + i });
				}

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from EntityA").ExecuteUpdate();
				session.CreateQuery("delete from EntityB").ExecuteUpdate();

				transaction.Commit();
			}
		}

		private const string QueryString =
			"select x from NHibernate.Test.NHSpecificTest.GH1167.INamed x";

		[Test]
		public void PolymorphicQueryOnUnmappedInterfaceRespectsMaxResults()
		{
			using (var session = OpenSession())
			{
				var results = session.CreateQuery(QueryString).SetMaxResults(5).List();

				Assert.That(results.Count, Is.EqualTo(5));
			}
		}

		[Test]
		public void PolymorphicQueryOnUnmappedInterfaceRespectsFirstResultAndMaxResults()
		{
			using (var session = OpenSession())
			{
				// 2 * CountPerClass rows exist in total; skip the first 15, take up to 10 -> 5 remain.
				var results = session.CreateQuery(QueryString).SetFirstResult(15).SetMaxResults(10).List();

				Assert.That(results.Count, Is.EqualTo(5));
			}
		}
	}
}
