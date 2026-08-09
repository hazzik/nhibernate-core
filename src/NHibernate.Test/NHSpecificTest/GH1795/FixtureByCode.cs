using System.Linq;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1795
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.GenerateStatistics, "true");
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<A>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.ManyToOne(x => x.B, m => m.Lazy(LazyRelation.NoProxy));
				});

			mapper.Class<B>(
				rc =>
				{
					rc.Id(x => x.Id);
					rc.Property(x => x.Name, m => m.Lazy(true));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var b = new B { Id = 1, Name = "TX" };
			session.Save(b);
			session.Save(new A { Id = 1, B = b });
			session.Save(new A { Id = 2, B = b });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from A").ExecuteUpdate();
			session.CreateQuery("delete from B").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void DoesNotEagerLoadNoProxyAssociationWhenItRepeatsInTheResultSet()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var list = session.Query<A>().Where(x => x.B.Name == "TX").ToList();

				Assert.That(list, Has.Count.EqualTo(2));

				// Neither A refers to B by anything other than its identifier at this point, so B
				// should stay an uninitialized no-proxy reference. It must not be loaded from the
				// database just because it is the target of two of the A rows just fetched.
				var entityStatistics = Sfi.Statistics.GetEntityStatistics(typeof(B).FullName);
				Assert.That(
					entityStatistics.LoadCount,
					Is.EqualTo(0),
					"B should stay lazy (no-proxy) and not be eagerly loaded just because it appears more than once in the result set");
			}
		}
	}
}
