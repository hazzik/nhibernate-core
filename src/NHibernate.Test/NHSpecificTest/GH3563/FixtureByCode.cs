using System.Collections.Generic;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3563
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Status, m => m.Type<EnumStringType<StatusEnum>>());
			});
			mapper.Import<StatusEnum>();

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Entity { Status = StatusEnum.Active });

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
		public void QueryingByStaticEnumFieldGeneratesValidSql()
		{
			using var session = OpenSession();

			// StatusEnum.Active is embedded as a literal in the generated SQL through
			// EnumStringType.ObjectToSQLString, which must produce a quoted string
			// literal (e.g. 'Active'), not the bare word Active.
			var query = session.CreateQuery("from Entity e where e.Status = StatusEnum.Active");

			IList<Entity> list = null;
			Assert.That(() => list = query.List<Entity>(), Throws.Nothing);
			Assert.That(list, Has.Count.EqualTo(1));
		}
	}
}
