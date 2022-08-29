﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Dialect;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2242
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FormulaTestAsync : BugTestCase
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect as MsSql2005Dialect != null;
		}

		[Test]
		public async Task FormulaOfEscapedDomainClassShouldBeRetrievedCorrectlyAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					var entity = new EscapedFormulaDomainClass();
					entity.Id = 1;
					await (session.SaveAsync(entity));

					await (transaction.CommitAsync());
				}

				session.Clear();

				using (ITransaction transaction = session.BeginTransaction())
				{
					var entity = await (session.GetAsync<EscapedFormulaDomainClass>(1));

					Assert.AreEqual(1, entity.Formula);
					await (session.DeleteAsync(entity));

					await (transaction.CommitAsync());
				}
			}
		}

		[Test]
		public async Task FormulaOfUnescapedDomainClassShouldBeRetrievedCorrectlyAsync()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					var entity = new UnescapedFormulaDomainClass();
					entity.Id = 1;
					await (session.SaveAsync(entity));

					await (transaction.CommitAsync());
				}

				session.Clear();

				using (ITransaction transaction = session.BeginTransaction())
				{
					var entity = await (session.GetAsync<UnescapedFormulaDomainClass>(1));

					Assert.AreEqual(1, entity.Formula);
					await (session.DeleteAsync(entity));
					await (transaction.CommitAsync());
				}
			}
		}
	}
}
