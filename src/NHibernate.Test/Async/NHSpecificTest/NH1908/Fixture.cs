﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1908
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		[Test]
		public async Task QueryPropertyInBothFilterAndQueryAsync()
		{
			using (ISession s = OpenSession())
			{
				s.EnableFilter("validity")
					.SetParameter("date", DateTime.Now);

				await (s.CreateQuery(@"
				select 
					inv.ID
				from 
					Invoice inv
						join inv.Category cat with cat.ValidUntil > :now
						left join cat.ParentCategory parentCat
				where
					inv.ID = :invId
					and inv.Issued < :now
				")
					.SetDateTime("now", DateTime.Now)
					.SetInt32("invId", -999)
					.ListAsync());
			}
		}

		[Test]
		public async Task QueryPropertyInBothFilterAndQueryUsingWithAsync()
		{
			using (ISession s = OpenSession())
			{
				s.EnableFilter("validity")
					.SetParameter("date", DateTime.Now);

				await (s.CreateQuery(@"
				select 
					inv.ID
				from 
					Invoice inv
						join inv.Category cat with cat.ValidUntil > :now
						left join cat.ParentCategory parentCat with parentCat.ID != :myInt
				where
					inv.ID = :invId
					and inv.Issued < :now
				")
					.SetDateTime("now", DateTime.Now)
					.SetInt32("invId", -999)
					.SetInt32("myInt", -888)
					.ListAsync());
			}
		}
	}
}
