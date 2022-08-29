﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Criterion;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2469
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		[Test]
		public async Task ShouldNotThrowSqlExceptionAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var criteria = session.CreateCriteria(typeof(Entity2), "e2")
					.CreateAlias("e2.Entity1", "e1")
					.Add(Restrictions.Eq("e1.Foo", 0));

				Assert.AreEqual(0, (await (criteria.ListAsync<Entity2>())).Count);
			}
		}
	}
}
