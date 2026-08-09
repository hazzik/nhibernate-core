using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3591
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
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Entity { Id = 1, Name = "One" });
			session.Save(new Entity { Id = 2, Name = "Two" });
			session.Save(new Entity { Id = 3, Name = "Three" });

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
		public void NestedConditionalInProjectionIsSelectedAsSingleColumn()
		{
			using var session = OpenSession();
			using var sqlLog = new SqlLogSpy();

			var result = session
				.Query<Entity>()
				.Select(x => new { x.Id, NameSeq = x.Name == "One" ? 1 : x.Name == "Two" ? 2 : 0 })
				.OrderBy(x => x.NameSeq)
				.ToList();

			Assert.That(result.Select(x => x.NameSeq), Is.EqualTo(new[] { 0, 1, 2 }));

			var sql = sqlLog.GetWholeLog();
			Assert.That(sql, Does.Not.Contain("col_2_0_"), "the conditional must be projected as a single column, not split into one column per comparison");
		}

		[Test]
		public void NestedConditionalInProjectionSupportsDistinct()
		{
			using var session = OpenSession();

			var result = session
				.Query<Entity>()
				.Select(x => x.Name == "One" ? 1 : x.Name == "Two" ? 2 : 0)
				.Distinct()
				.OrderBy(x => x)
				.ToList();

			Assert.That(result, Is.EqualTo(new[] { 0, 1, 2 }));
		}
	}
}
