using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1061
{
	// NH-1848 (GH-1061): NHibernate does not obey insert="false" (nor update="false") on
	// properties mapped inside a composite-element of a collection: the property is still
	// included in the generated collection row INSERT (and UPDATE) statement.
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Name);
				rc.Bag(
					x => x.Children,
					cam =>
					{
						cam.Key(k => k.Column("ParentId"));
						cam.Cascade(Mapping.ByCode.Cascade.All);
					},
					cr => cr.Component(ce =>
					{
						ce.Property(x => x.Label);
						ce.Property(
							x => x.ReadOnlyValue,
							m =>
							{
								m.Insert(false);
								m.Update(false);
							});
					}));
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			foreach (var parent in session.Query<Parent>().ToList())
			{
				session.Delete(parent);
			}

			session.Flush();
			transaction.Commit();
		}

		[Test]
		public void DoesNotWriteInsertFalseCompositeElementPropertyOnInsert()
		{
			var parent = new Parent { Name = "Parent" };
			parent.Children.Add(new ChildValue { Label = "child", ReadOnlyValue = "must-not-be-written" });

			using var spy = new SqlLogSpy();
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(parent);
				session.Flush();
				transaction.Commit();
			}

			var wholeLog = spy.GetWholeLog();
			Assert.That(
				wholeLog,
				Does.Not.Contain("ReadOnlyValue"),
				"a composite-element property mapped with insert=\"false\" must not appear in the generated collection row INSERT statement");
		}
	}
}
