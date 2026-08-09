using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1156
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<ExternalField>(
				rc =>
				{
					rc.ComponentAsId(
						x => x.Id,
						m =>
						{
							m.Property(y => y.CommonId);
							m.Property(y => y.Id);
						});

					rc.Property(x => x.SomeProp);
				});

			mapper.Class<Field>(
				rc =>
				{
					rc.ComponentAsId(
						x => x.Id,
						m =>
						{
							m.Property(y => y.CommonId);
							m.Property(y => y.Id);
						});

					rc.Property(x => x.ExternalFieldRefId, m => m.Column("ExternalFieldId"));

					// Mirrors the original report: the first column (CommonId) is shared with this
					// entity's own composite id, the second (ExternalFieldId) is a plain nullable
					// column. The association is read-only since its columns are already written by
					// the id component and the plain property above.
					rc.ManyToOne(
						x => x.ExternalField,
						m =>
						{
							m.Columns(
								c => c.Name("CommonId"),
								c => c.Name("ExternalFieldId"));
							m.Insert(false);
							m.Update(false);
							m.NotFound(NotFoundMode.Ignore);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new ExternalField { Id = new ExternalFieldId { CommonId = 1, Id = 1 }, SomeProp = "X" });

			// A genuine reference: both foreign key columns (CommonId, ExternalFieldId) point to the
			// ExternalField above.
			session.Save(new Field { Id = new FieldId { CommonId = 1, Id = 1 }, ExternalFieldRefId = 1 });

			// No real reference: CommonId is populated only because it is shared with this Field's own
			// id, but ExternalFieldId is null, so there is no corresponding ExternalField.
			session.Save(new Field { Id = new FieldId { CommonId = 1, Id = 2 }, ExternalFieldRefId = null });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from System.Object").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ManyToOneWithCompositeIdIsNullOnlyWhenAllForeignKeyColumnsAreNull()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var fieldsWithExternalField = session
				.QueryOver<Field>()
				.WhereRestrictionOn(f => f.ExternalField).IsNotNull
				.List();

			Assert.That(
				fieldsWithExternalField.Select(f => f.Id),
				Is.EquivalentTo(new[] { new FieldId { CommonId = 1, Id = 1 } }));

			transaction.Commit();
		}
	}
}
