using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1186
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
					rc.Id(
						x => x.Id,
						m => m.Generator(
							Generators.HighLow,
							gm => gm.Params(new { table = "`GH1186HiLo`", column = "`NextHi`", max_lo = 5 })));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Entity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void QuotedHiLoTableAndColumnNamesAreHonoured()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var entity = new Entity { Name = "test" };
			session.Save(entity);
			transaction.Commit();

			Assert.That(entity.Id, Is.GreaterThan(0));
		}
	}
}
