using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1043
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Contact>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Name);
			});

			mapper.Class<EntityCriticalAttribute>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Discriminator(d =>
				{
					d.Column("dtype");
					d.Length(640);
					d.NotNullable(true);
				});
			});

			// the reporter's supported way of plugging a conformist customizer class into the
			// model mapper (ModelMapper.AddMapping<T>() where T : IConformistHoldersProvider)
			mapper.AddMapping(new ContactEditingLevelAttributeMapping());

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from EntityCriticalAttribute").ExecuteUpdate();
			session.CreateQuery("delete from Contact").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void MapsAttributePropertyOfGenericBaseClass()
		{
			long attributeId;

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var contact = new Contact { Name = "Jane" };
				session.Save(contact);

				var attribute = new ContactEditingLevelAttribute
				{
					Entity = contact,
					Attribute = EditingLevel.ReadOnly
				};
				session.Save(attribute);
				attributeId = attribute.Id;

				transaction.Commit();
			}

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var attribute = session.Get<ContactEditingLevelAttribute>(attributeId);

				Assert.That(attribute.Attribute, Is.EqualTo(EditingLevel.ReadOnly),
					"The 'Attribute' property, declared on the generic base class, was not persisted/loaded correctly.");

				transaction.Commit();
			}
		}
	}
}
