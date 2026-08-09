using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1138
{
	[TestFixture]
	public class Fixture
	{
		[Test]
		public void NullEqualsNullIsTrue()
		{
			SchemaAutoAction action = null;

			Assert.That(action == null, Is.True);
			Assert.That(null == action, Is.True);
			Assert.That(action != null, Is.False);
			Assert.That(null != action, Is.False);
		}
	}
}
