using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1153
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			for (var i = 1; i <= 10; i++)
			{
				session.Save(new Entity { Id = i });
			}

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Entity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ChainedTakeUsesTheSmallestValue()
		{
			using var session = OpenSession();

			var query = (from e in session.Query<Entity>()
						 orderby e.Id
						 select e.Id).Take(5).Take(6).ToList();

			Assert.That(query.Count, Is.EqualTo(5));
		}

		[Test]
		public void ChainedTakeUsesTheSmallestValueRegardlessOfOrder()
		{
			using var session = OpenSession();

			var query = (from e in session.Query<Entity>()
						 orderby e.Id
						 select e.Id).Take(6).Take(5).ToList();

			Assert.That(query.Count, Is.EqualTo(5));
		}
	}
}
