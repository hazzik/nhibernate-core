using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1334
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Book>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Code);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Book { Code = "1" });
			session.Save(new Book { Code = "12" });
			session.Save(new Book { Code = "3" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Book").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanQueryUsingAnyOnParameterListInsideWhere()
		{
			var filterList = new List<string> { "1", "2" };

			using var session = OpenSession();

			var list = session.Query<Book>()
				.Where(x => filterList.Any(s => s.StartsWith(x.Code)))
				.ToList();

			Assert.That(list.Select(x => x.Code), Is.EquivalentTo(new[] { "1" }));
		}
	}
}
