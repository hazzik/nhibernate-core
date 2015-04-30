using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3490
{
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			//Three mappings for three diffrent EntityNames, only the last one persists if entity-name is not used as part of the "key" defining the mapping
			//so note the order here affects the tests outcome
			mapper.AddMapping<SomeEntityLiteMap>();
			mapper.AddMapping<SomeEntityAnotherTableMap>();
			mapper.AddMapping<SomeEntityFullMap>();
			
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
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public void ShouldBeAbleToUseEntityNameToMapTableToTwoDiffrentClasses()
		{
			//Create entity through "SomeEntityFull" mapping
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var e1 = new SomeEntity { OneProperty = "SomeValue", AnotherProperty = "SomeValue" };
				session.Save("SomeEntityFull", e1);

				session.Flush();
				transaction.Commit();
			}

			//Perform query as "SomeEntityLite"
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var entity = session.CreateCriteria("SomeEntityLite")
									.UniqueResult<SomeEntity>();

				Assert.AreEqual(null, entity.AnotherProperty);
			}
		}

		[Test]
		public void ShouldBeAbleToUseEntityNameToMapClassFromTwoDiffrentTables()
		{
			//Create entity with "SomeEntityAnotherTable" mapping
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var e2 = new SomeEntity { AnotherProperty = "SomeValue" };
				session.Save("SomeEntityAnotherTable", e2);

				session.Flush();
				transaction.Commit();
			}

			//Perform query with "SomeEntityAnotherTable"
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var entity = session.CreateCriteria("SomeEntityAnotherTable")
									.UniqueResult<SomeEntity>();

				Assert.AreEqual(null, entity.OneProperty);
			}
		}
	}
}
