using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2908
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.AddMapping<PersonMapping>();
			mapper.AddMapping<ProgrammerMapping>();
			mapper.AddMapping<ManagerMapping>();
			mapper.AddMapping<GroupMapping>();
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from NHibernate.Test.NHSpecificTest.GH2908.Group").ExecuteUpdate();
				session.CreateQuery("delete from System.Object").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void ShouldBeAbleToSaveGroup()
		{
			using (var session = Sfi.OpenStatelessSession())
			using (var transaction = session.BeginTransaction())
			{
				var programmer = new Programmer();
				var manager = new Manager();

				session.Insert(programmer);
				session.Insert(manager);
				session.Insert(new Group { Leader = programmer });
				session.Insert(new Group { Leader = manager });

				transaction.Commit();
			}
		}
	}
}
