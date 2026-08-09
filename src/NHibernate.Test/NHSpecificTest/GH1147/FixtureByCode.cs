using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1147
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<SrcObj>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Member);
				});

			mapper.Class<DestObj>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Member);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new SrcObj { Id = 1, Member = "SomeValue" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from DestObj").ExecuteUpdate();
			session.CreateQuery("delete from SrcObj").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void InsertSelectWithPropertyNamedAsOperator()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// "Member" is also the HQL "member of" operator keyword; it must still be usable
			// as a plain property/column name in an "insert into ... select" statement.
			session.CreateQuery("insert into DestObj (Id, Member) select Id, Member from SrcObj")
				.ExecuteUpdate();

			transaction.Commit();

			using var verifySession = OpenSession();
			var dest = verifySession.Query<DestObj>().ToList();

			Assert.That(dest, Has.Count.EqualTo(1));
			Assert.That(dest[0].Member, Is.EqualTo("SomeValue"));
		}
	}
}
