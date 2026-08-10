using System;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1208
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		private static readonly DateTime AsOf = new DateTime(2020, 1, 1);

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var validCategory = new Category { Name = "Valid", ValidUntil = new DateTime(2999, 1, 1) };
			var expiredCategory = new Category { Name = "Expired", ValidUntil = new DateTime(2000, 1, 1) };

			session.Save(validCategory);
			session.Save(expiredCategory);

			session.Save(new Invoice { Name = "WithValidCategory", Category = validCategory });
			session.Save(new Invoice { Name = "WithExpiredCategory", Category = expiredCategory });
			session.Save(new Invoice { Name = "WithoutCategory", Category = null });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Delete("from Invoice");
			session.Delete("from Category");

			transaction.Commit();
		}

		[Test]
		public void FilterOnNullableManyToOneDoesNotExcludeParentRow()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.EnableFilter("validCategory").SetParameter("asOf", AsOf);

			var invoices = session.CreateQuery("select i from Invoice i left join fetch i.Category order by i.Name").List<Invoice>();

			transaction.Commit();

			// The filter on Category must be applied on the join (Invoice LEFT OUTER JOIN Category),
			// not moved to the query's WHERE clause. All three invoices must still be returned:
			// applying it to the WHERE clause turns the left outer join into an inner join and
			// drops invoices whose category is missing or filtered out. A category that fails the
			// filter must come back as null on the invoice, not be resolved anyway through a
			// separate, unfiltered fetch keyed off the raw foreign key value.
			Assert.That(invoices.Count, Is.EqualTo(3), "Filtering on the WHERE clause incorrectly dropped invoice rows");

			var withValidCategory = invoices.Single(i => i.Name == "WithValidCategory");
			var withExpiredCategory = invoices.Single(i => i.Name == "WithExpiredCategory");
			var withoutCategory = invoices.Single(i => i.Name == "WithoutCategory");

			Assert.That(withValidCategory.Category, Is.Not.Null);
			Assert.That(withExpiredCategory.Category, Is.Null, "Category failing the filter should be nulled out, not exclude the invoice");
			Assert.That(withoutCategory.Category, Is.Null);
		}
	}
}
