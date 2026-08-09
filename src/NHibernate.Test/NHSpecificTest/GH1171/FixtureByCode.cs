using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1171
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Classification>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Type);
				});

			mapper.Class<SpecialTemplate>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.ManyToOne(
						x => x.TemplateType,
						m =>
						{
							m.Fetch(FetchKind.Join);
							m.Lazy(LazyRelation.NoLazy);
						});
				});

			mapper.Class<TemplateGroup>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.ManyToOne(
						x => x.MySpecialTemplate,
						m =>
						{
							m.Fetch(FetchKind.Join);
							m.Lazy(LazyRelation.NoLazy);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		private int _templateGroupId;

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var classification = new Classification { Type = ClassificationType.Company };
			var specialTemplate = new SpecialTemplate { TemplateType = classification };
			var templateGroup = new TemplateGroup { MySpecialTemplate = specialTemplate };

			session.Save(classification);
			session.Save(specialTemplate);
			session.Save(templateGroup);

			transaction.Commit();

			_templateGroupId = templateGroup.Id;
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from TemplateGroup").ExecuteUpdate();
			session.CreateQuery("delete from SpecialTemplate").ExecuteUpdate();
			session.CreateQuery("delete from Classification").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void LoadsNestedJoinFetchedPropertyWithoutTriggeringValidationOnPartiallyHydratedEntity()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var templateGroup = session.Get<TemplateGroup>(_templateGroupId);

			Assert.That(templateGroup.MySpecialTemplate, Is.Not.Null);
			Assert.That(templateGroup.MySpecialTemplate.TemplateType, Is.Not.Null);
			Assert.That(templateGroup.MySpecialTemplate.TemplateType.Type, Is.EqualTo(ClassificationType.Company));

			transaction.Commit();
		}
	}
}
