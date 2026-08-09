using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1192
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.ManyToOne(x => x.NestedField, m => m.Column("NestedFieldId"));
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Value);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				// No NestedField set: the association is null, causing an outer join
				// that yields a null value for Child.Value in the projected row.
				session.Save(new Parent { Id = 1 });
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from Parent").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void SelectingPropertyOfNullNestedAssociationDoesNotThrow()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				ParentDto dto = null;
				Assert.DoesNotThrow(
					() => dto = session
						.Query<Parent>()
						.Select(p => new ParentDto { Id = p.Id, Field = p.NestedField.Value })
						.Single());

				Assert.That(dto.Field, Is.EqualTo(0));
			}
		}
	}
}
