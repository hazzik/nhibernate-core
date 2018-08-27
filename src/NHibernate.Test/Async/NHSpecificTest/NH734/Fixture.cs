﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH734
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		[TestAttribute]
		public async Task LimitProblemAsync()
		{
			using (ISession session = Sfi.OpenSession())
			{
				ICriteria criteria = session.CreateCriteria(typeof(MyClass));
				criteria.SetMaxResults(100);
				criteria.SetFirstResult(0);
				try
				{
					session.BeginTransaction();
					IList result = await (criteria.ListAsync());
					await (session.Transaction.CommitAsync());
				}
				catch
				{
					if (session.Transaction != null)
					{
						await (session.Transaction.RollbackAsync());
					}
					throw;
				}
			}
		}
	}
}