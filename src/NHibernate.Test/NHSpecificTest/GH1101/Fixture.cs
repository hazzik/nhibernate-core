using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1101
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
				rc.Property(x => x.Name);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Entity { Name = "test" });

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
		public void LockModeIsAppliedWhenCriteriaHasProjection()
		{
			using var log = new SqlLogSpy();
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var ids = session.CreateCriteria<Entity>()
				.SetProjection(Property.ForName("Id"))
				.SetLockMode(LockMode.Upgrade)
				.List<int>();

			transaction.Commit();

			Assert.That(ids, Has.Count.EqualTo(1));
			Assert.That(log.GetWholeLog(), Does.Contain("for update").IgnoreCase,
				"The lock mode was not applied to the generated SQL when the criteria has a projection.");
		}
	}
}
