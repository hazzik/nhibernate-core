﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2241
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				var country = new Country { CountryCode = "SE", CountryName = "Sweden" };
				session.Save(country);
				tran.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				session.Delete("from Country");
				session.Delete("from User");
				tran.Commit();
			}
		}

		[Test]
		public async Task CanInsertUsingStatelessEvenWhenAssociatedEntityHasCacheStategyAsync()
		{
			using (var ss = Sfi.OpenStatelessSession())
			using (var tx = ss.BeginTransaction())
			{
				var user = new User();
				user.Country = new Country { CountryCode = "SE", CountryName = "Sweden" };

				await (ss.InsertAsync(user));
				await (tx.CommitAsync());
			}
		}
	}
}
