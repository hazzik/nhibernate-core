using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3363
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Mother>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Discriminator(d => d.Column("Kind"));
					rc.Property(x => x.Name);
				});

			mapper.Class<Thing1>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Description);
				});

			mapper.Class<Thing2>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Description);
				});

			mapper.Subclass<Child1>(
				rc =>
				{
					rc.DiscriminatorValue("1");
					rc.ManyToOne(
						x => x.Thing,
						m =>
						{
							m.Column("ThingId");
							m.NotFound(NotFoundMode.Ignore);
						});
				});

			mapper.Subclass<Child2>(
				rc =>
				{
					rc.DiscriminatorValue("2");
					rc.ManyToOne(
						x => x.Thing,
						m =>
						{
							m.Column("ThingId");
							m.NotFound(NotFoundMode.Ignore);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var thing1 = new Thing1 { Id = "00001", Description = "Thing1 A" };
			var thing2 = new Thing2 { Id = "00002", Description = "Thing2 A" };
			session.Save(thing1);
			session.Save(thing2);

			session.Save(new Child1 { Name = "Child1 A", Thing = thing1 });
			session.Save(new Child2 { Name = "Child2 A", Thing = thing2 });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from System.Object").ExecuteUpdate();
			session.CreateQuery("delete from Thing1").ExecuteUpdate();
			session.CreateQuery("delete from Thing2").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void QueryOnSubclassAssociationWithSamePropertyNameAsSiblingSubclass()
		{
			using var session = OpenSession();

			var result = session.Query<Mother>()
				.Where(k => k is Child1 && (k as Child1).Thing.Id == "00001")
				.ToList();

			Assert.That(result, Has.Count.EqualTo(1), "Should have found the Child1 entity referencing Thing1 with id 00001");
			Assert.That(result[0], Is.InstanceOf<Child1>());
		}
	}
}
