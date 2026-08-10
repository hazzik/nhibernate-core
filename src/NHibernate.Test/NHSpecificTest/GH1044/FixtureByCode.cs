using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1044
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(rc =>
			{
				// Mapped in the reverse order to the class declaration order
				// (IntKey then StrKey) on purpose: the generated column order
				// should follow this call order, not reflection order.
				rc.ComposedId(cm =>
				{
					cm.Property(x => x.IntKey);
					cm.Property(x => x.StrKey);
				});
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Parent").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void InsertRespectsMappedColumnOrder()
		{
			using var spy = new SqlLogSpy();
			using (var session = OpenSession())
			using (var t = session.BeginTransaction())
			{
				session.Save(new Parent { StrKey = "abc", IntKey = 1 });
				t.Commit();
			}

			var sql = spy.GetWholeLog();
			var intKeyIndex = sql.IndexOf("IntKey", StringComparison.OrdinalIgnoreCase);
			var strKeyIndex = sql.IndexOf("StrKey", StringComparison.OrdinalIgnoreCase);

			Assert.That(intKeyIndex, Is.GreaterThanOrEqualTo(0), "IntKey column not found in generated SQL.");
			Assert.That(strKeyIndex, Is.GreaterThanOrEqualTo(0), "StrKey column not found in generated SQL.");
			Assert.That(intKeyIndex, Is.LessThan(strKeyIndex),
				"Column order should follow the order properties were mapped in ComposedId (IntKey, StrKey), not their declaration order in the class.");
		}
	}
}
