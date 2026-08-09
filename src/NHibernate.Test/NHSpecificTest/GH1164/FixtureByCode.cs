using System.Linq;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1164
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.LinqToHqlGeneratorsRegistry<MyLinqToHqlGeneratorsRegistry>();
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<MyEntity>(
				rc =>
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

			session.Save(new MyEntity {Name = "Test"});

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from MyEntity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CustomToStringGeneratorRegisteredForOverrideIsUsed()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// MyEntity.ToString() is overridden and a dedicated LINQ generator has been registered
			// for that specific override. It must be used instead of the generic ToString()
			// generator that casts the entity to a string via the HQL str() function.
			var count = session.Query<MyEntity>().Count(e => e.ToString() == CustomToStringGenerator.Marker);

			Assert.That(count, Is.EqualTo(1), "The custom LINQ generator registered for MyEntity.ToString() was not used.");
		}
	}
}
