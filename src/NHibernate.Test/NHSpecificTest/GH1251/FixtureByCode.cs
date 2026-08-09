using System.Collections;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1251
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<ServiceItemBase>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Discriminator(x => x.Column("TypeLiteral"));
				rc.Property(x => x.CurrencyId);
				rc.Abstract(true);
			});
			mapper.Subclass<SI_Discount>(rc =>
			{
				rc.DiscriminatorValue("Discount");
				rc.Property(x => x.Amount);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new SI_Discount { CurrencyId = 1, Amount = 10 });

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void ClassParameterIsConvertedToDiscriminatorValue()
		{
			using (var session = OpenSession())
			{
				var query = session.CreateQuery("from ServiceItemBase s where s.class = :t");
				query.SetParameter("t", typeof(SI_Discount));

				IList result = query.List();

				Assert.That(result.Count, Is.EqualTo(1));
			}
		}
	}
}
