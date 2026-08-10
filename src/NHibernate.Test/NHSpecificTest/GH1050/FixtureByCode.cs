using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1050
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<School>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
					rc.ManyToOne(x => x.Janitor, m => m.Column("JanitorId"));
				});

			mapper.Class<Janitor>(
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

			session.Save(new School { Name = "Hogwarts", Janitor = null });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from School").ExecuteUpdate();
			session.CreateQuery("delete from Janitor").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ProjectionWithNullReferenceYieldsNullNestedDto()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var dto = session
				.Query<School>()
				.Select(e => new SchoolDto { Name = e.Name, Janitor = new JanitorDto { Name = e.Janitor.Name } })
				.Single();

			transaction.Commit();

			Assert.That(dto.Janitor, Is.Null, "Janitor should be null when the underlying entity reference is null, matching direct entity load behaviour.");
		}
	}
}
