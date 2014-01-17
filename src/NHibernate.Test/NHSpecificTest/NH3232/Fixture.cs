using System;
using System.Linq;
using NHibernate.Dialect;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3232
{
	[TestFixture]
	public class Fixture :  BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is MsSqlCe40Dialect;
		}

		protected override void OnSetUp()
		{
			const string user = "test";

			using (var session = sessions.OpenSession())
			using (session.BeginTransaction())
			{
				var page = new Page
					{
						Title = "Blah",
						Tags = "tag1 tag2 tag3",
						CreatedBy = user,
						CreatedOn = DateTime.Now,
						ModifiedOn = DateTime.Now,
						ModifiedBy = user
					};
				session.Save(page);
				session.Save(new PageContent
					{
						VersionNumber = 1,
						Text = "lots of content",
						EditedBy = user,
						EditedOn = DateTime.Now,
						Page = page
					});

				session.Transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = sessions.OpenSession())
			using (session.BeginTransaction())
			{
				session.Delete("from System.Object");
				session.Transaction.Commit();
			}
		}

		[Test]
		public void AbleToDoPagingWithMsSqlCe40Dialect()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var page = session.Query<Page>().First();
				var query = session
					.CreateQuery("FROM PageContent fetch all properties WHERE Page.Id=:Id AND VersionNumber=(SELECT max(VersionNumber) FROM PageContent WHERE Page.Id=:Id)")
					.SetCacheable(true)
					.SetInt32("Id", page.Id)
					.SetMaxResults(1);

				var content = query.UniqueResult<PageContent>();
				Console.WriteLine(content.Text);
				Console.Read();
			}
		}
	}
}
