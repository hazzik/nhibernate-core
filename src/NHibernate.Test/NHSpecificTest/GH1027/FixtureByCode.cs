using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1027
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<TextResource>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Map(
						x => x.Translations,
						m =>
						{
							m.Key(k => k.Column("TextResourceId"));
							m.Cascade(Mapping.ByCode.Cascade.All | Mapping.ByCode.Cascade.DeleteOrphans);
						},
						k => k.Element(e => e.Column("TranslationKey")),
						r => r.OneToMany());
				});

			mapper.Class<Translation>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.TextValue);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var textResource = new TextResource();
			textResource.Translations.Add("en", new Translation { TextValue = "Hello" });
			textResource.Translations.Add("fr", new Translation { TextValue = "Bonjour" });
			session.Save(textResource);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Translation").ExecuteUpdate();
			session.CreateQuery("delete from TextResource").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void SelectingChildPropertyThroughIndexedCollectionWorks()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var result = session.Query<TextResource>().Select(res => res.Translations["en"].TextValue).ToList();

			Assert.That(result, Is.EquivalentTo(new[] { "Hello" }));
		}

		[Test]
		public void GroupingByChildPropertyThroughIndexedCollectionWorks()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var result = session.Query<TextResource>()
				.GroupBy(x => x.Translations["en"].TextValue)
				.Select(group => new { group.Key, Count = group.Count() })
				.ToList();

			Assert.That(result.Select(x => x.Key), Is.EquivalentTo(new[] { "Hello" }));
			Assert.That(result.Single().Count, Is.EqualTo(1));
		}
	}
}
