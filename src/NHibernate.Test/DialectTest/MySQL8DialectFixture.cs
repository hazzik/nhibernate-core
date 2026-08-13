using NHibernate.Dialect;
using NHibernate.SqlTypes;
using NUnit.Framework;

namespace NHibernate.Test.DialectTest
{
	[TestFixture]
	public class MySQL8DialectFixture
	{
		[Test]
		public void CastsToFloatingPointTypes()
		{
			var dialect = new MySQL8Dialect();

			Assert.That(dialect.GetCastTypeName(SqlTypeFactory.Double), Is.EqualTo("DOUBLE").IgnoreCase, "Double");
			Assert.That(dialect.GetCastTypeName(SqlTypeFactory.Single), Is.EqualTo("FLOAT").IgnoreCase, "Single");
		}

		[Test]
		public void MySQL5CastsToDecimal()
		{
			// MySQL accepts the floating point types as cast targets only since 8.0.17.
			var dialect = new MySQL5Dialect();

			Assert.That(dialect.GetCastTypeName(SqlTypeFactory.Double), Is.EqualTo("DECIMAL(19,5)").IgnoreCase, "Double");
			Assert.That(dialect.GetCastTypeName(SqlTypeFactory.Single), Is.EqualTo("DECIMAL(19,5)").IgnoreCase, "Single");
		}
	}
}
