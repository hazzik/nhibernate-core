using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1045
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Product>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Attributes, m => m.Column(c => c.SqlType("jsonb")));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			// The bug is about PostgreSQL's "?" jsonb containment operator being mistaken
			// for an ADO.NET parameter placeholder.
			return dialect is PostgreSQLDialect;
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateSQLQuery("insert into Product (Id, Attributes) values (1, '{\"color\": \"red\"}')")
				.ExecuteUpdate();
			session.CreateSQLQuery("insert into Product (Id, Attributes) values (2, '{\"size\": \"large\"}')")
				.ExecuteUpdate();

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateSQLQuery("delete from Product").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanUseJsonExistsOperatorInSqlRestriction()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var products = session
				.CreateCriteria<Product>()
				.Add(Expression.Sql("{alias}.Attributes ? 'color'"))
				.List<Product>();

			Assert.That(products.Select(p => p.Id), Is.EquivalentTo(new[] { 1 }));

			transaction.Commit();
		}
	}
}
