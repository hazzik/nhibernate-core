﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH473
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsEmptyInsertsOrHasNonIdentityNativeGenerator;
		}

		protected override void OnSetUp()
		{
			using (var session = this.OpenSession())
			using (var tran = session.BeginTransaction())
			{
				var parent = new Parent();
				var child1 = new Child { Name = "Fabio" };
				var child2 = new Child { Name = "Ayende" };
				var child3 = new Child { Name = "Dario" };
				parent.Children.Add(child1);
				parent.Children.Add(child2);
				parent.Children.Add(child3);
				session.Save(parent);
				tran.Commit();
			}
		}
		protected override void OnTearDown()
		{
			using (var session = this.OpenSession())
			using (var tran = session.BeginTransaction())
			{
				session.Delete("from Parent");
				tran.Commit();
			}
		}

		[Test]
		public async Task ChildItemsGetInOrderWhenUsingFetchJoinAsync()
		{
			using (var session = this.OpenSession())
			using (var tran = session.BeginTransaction())
			{
				var result = await (session.CreateCriteria(typeof(Parent))
					.Fetch("Children")
					.ListAsync<Parent>());
				Assert.That(result[0].Children[0].Name, Is.EqualTo("Ayende"));
				Assert.That(result[0].Children[1].Name, Is.EqualTo("Dario"));
				Assert.That(result[0].Children[2].Name, Is.EqualTo("Fabio"));
			}
		}

		[Test]
		public async Task ChildItemsGetInOrderWhenUsingFetchJoinUniqueResultAsync()
		{
			using (var session = this.OpenSession())
			using (var tran = session.BeginTransaction())
			{
				var result = await (session.CreateCriteria(typeof(Parent))
					.Fetch("Children")
					.UniqueResultAsync<Parent>());
				Assert.That(result.Children[0].Name, Is.EqualTo("Ayende"));
				Assert.That(result.Children[1].Name, Is.EqualTo("Dario"));
				Assert.That(result.Children[2].Name, Is.EqualTo("Fabio"));
			}
		}
	}
}
