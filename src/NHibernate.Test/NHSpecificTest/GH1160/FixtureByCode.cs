using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1160
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private System.Guid _basketId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Basket>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Name);
					rc.Set(
						x => x.Lines,
						m =>
						{
							m.Access(Accessor.Field);
							m.Key(
								k =>
								{
									k.Column("BasketId");
									k.OnDelete(OnDeleteAction.Cascade);
								});
							m.Cascade(Mapping.ByCode.Cascade.All | Mapping.ByCode.Cascade.DeleteOrphans);
							m.Inverse(true);
						},
						r => r.OneToMany());
				});

			mapper.Class<BasketLine>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Property(x => x.Description);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var basket = new Basket { Name = "Basket test" };
				basket.Lines.Add(new BasketLine { Description = "Line 1" });
				basket.Lines.Add(new BasketLine { Description = "Line 2" });

				_basketId = (System.Guid) session.Save(basket);
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from BasketLine").ExecuteUpdate();
				session.CreateQuery("delete from Basket").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void DeleteDoesNotInitializeCascadeDeleteCollection()
		{
			using (var sqlSpy = new SqlLogSpy())
			{
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					// Only the parent is loaded here; the Lines collection is mapped with
					// on-delete="cascade", so the database is responsible for removing the
					// child rows. NHibernate should not need to select them.
					var basket = session.Get<Basket>(_basketId);
					session.Delete(basket);
					transaction.Commit();
				}

				var wholeLog = sqlSpy.GetWholeLog();
				Assert.That(wholeLog, Does.Not.Contain("BasketLine"),
					"The BasketLine collection should not have been selected: the foreign key is mapped on-delete=\"cascade\", so the database removes the rows.");
			}
		}
	}
}
