﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2697
{
	using System.Threading.Tasks;
	[TestFixture]
	public class SampleTestAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession session = this.OpenSession())
			{
				ArticleGroupItem agrp_1 = new ArticleGroupItem();
				agrp_1.Name = "Article group 1";
				session.Save(agrp_1);
				ArticleGroupItem agrp_2 = new ArticleGroupItem();
				agrp_2.Name = "Article group 2";
				session.Save(agrp_2);
				session.Flush();

				ArticleItem article_1 = new ArticleItem();
				article_1.Articlegroup = agrp_1;
				article_1.Name = "Article 1 grp 1";
				article_1.IsFavorite = 0;
				session.Save("Article", article_1);

				ArticleItem article_2 = new ArticleItem();
				article_2.Articlegroup = agrp_1;
				article_2.Name = "Article 2 grp 1";
				article_2.IsFavorite = 1;
				session.Save("Article", article_2);

				ArticleItem article_3 = new ArticleItem();
				article_3.Articlegroup = agrp_2;
				article_3.Name = "Article 1 grp 2";
				article_3.IsFavorite = 0;
				session.Save("Article", article_3);

				session.Flush();
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();

			using (ISession session = this.OpenSession())
			{
				IList<ArticleItem> list = session.CreateCriteria("Article").List<ArticleItem>();
				foreach (ArticleItem item in list)
					session.Delete("Article", item);
				session.Flush();
			}

			//Articles where not removed (!?)
			//using (ISession session = this.OpenSession()) {
			//    string hql = "from Article";
			//    session.Delete(hql);
			//    session.Flush();
			//}

			using (ISession session = this.OpenSession())
			{
				string hql = "from ArticleGroupItem";
				session.Delete(hql);
				session.Flush();
			}
		}

		[Test]
		public async Task Can_GetListOfArticleGroupsAsync()
		{
			string HQL;
			IList<ArticleGroupItem> result;

			//add new
			using (ISession session = this.OpenSession())
			{
				ArticleGroupItem item = new ArticleGroupItem();
				item.Name = "Test article group";
				await (session.SaveAsync(item));
				await (session.FlushAsync());
			}

			HQL = "from ArticleGroupItem";
			using (ISession session = this.OpenSession())
			{
				result = await (session.CreateQuery(HQL).ListAsync<ArticleGroupItem>());
			}
			Assert.That(result.Count, Is.GreaterThan(0));
		}

		[Test]
		public async Task Can_GetListOfArticlesAsync()
		{
			string HQL;
			IList<ArticleItem> result;

			//add new
			using (ISession session = this.OpenSession())
			{
				ArticleItem item = new ArticleItem();
				item.Name = "Test article";
				item.IsFavorite = 0;
				await (session.SaveAsync("Article", item));
				await (session.FlushAsync());
			}

			//here first problem, no entities are returned <========
			HQL = "from Article";
			using (ISession session = this.OpenSession())
			{
				result = await (session.CreateQuery(HQL).ListAsync<ArticleItem>());
			}
			Assert.That(result.Count, Is.GreaterThan(0));
		}

		[Test]
		public async Task Can_SetArticleFavoriteWithHQL_NamedParamAsync()
		{
			string HQL;
			IList<ArticleItem> result;

			Int16 isFavValue = 1;

			//set  isFavorite for all articles
			HQL = "update Article a set a.IsFavorite= :Fav";
			using (ISession session = this.OpenSession())
			{
				await (session.CreateQuery(HQL)
							.SetInt16("Fav", isFavValue) //Exception !!
														 //.SetParameter("Fav", isFavValue) //Exception also !!
							.ExecuteUpdateAsync());

				await (session.FlushAsync());
			}

			//Check if some articles have isFavorite=1
			HQL = "from Article a where a.IsFavorite=1";
			using (ISession session = this.OpenSession())
			{
				result = await (session.CreateQuery(HQL).ListAsync<ArticleItem>());
			}
			Assert.That(result.Count, Is.GreaterThan(0));
		}
	}
}
