using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1343
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Employee>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Employee { Name = "John" });
			session.Save(new Employee { Name = "John" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Employee").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanCallFirstOnGroupByQuery()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var group = session.Query<Employee>().GroupBy(x => x.Name).First();

			Assert.That(group.Key, Is.EqualTo("John"));
			Assert.That(group.Count(), Is.EqualTo(2));
		}
	}
}
