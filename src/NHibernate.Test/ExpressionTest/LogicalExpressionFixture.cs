using System;
using NHibernate.Criterion;
using NHibernate.DomainModel;
using NHibernate.SqlCommand;
using NUnit.Framework;

namespace NHibernate.Test.ExpressionTest
{
	/// <summary>
	/// Test the LogicalExpression class.
	/// </summary>
	/// <remarks>
	/// There are no need for the subclasses AndExpression and OrExpression to have their own 
	/// TestFixtures because all they do is override one property.
	/// </remarks>
	[TestFixture]
	public class LogicalExpressionFixture : BaseExpressionFixture
	{
		[Test]
		public void LogicalSqlStringTest()
		{
			ISession session = factory.OpenSession();

			ICriterion orExpression =
				Expression.Or(Expression.IsNull("Address"), Expression.Between("Count", 5, 10));

			CreateObjects(typeof(Simple), session);
			SqlString sqlString = orExpression.ToSqlString(criteria, criteriaQuery);

			string expectedSql = "(sql_alias.address is null or sql_alias.count_ between ? and ?)";

			CompareSqlStrings(sqlString, expectedSql, 2);

			session.Close();
		}
	}
}
