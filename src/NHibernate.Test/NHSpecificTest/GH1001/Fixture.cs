using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1001
{
	// NH-2629: a <typedef> referenced from a <key-property> inside <composite-id> used to fail
	// with "Could not determine type for: <typedef name>", even though the same typedef name
	// worked fine on a plain <property>.
	[TestFixture]
	public class Fixture
	{
		[Test]
		public void TypedefCanBeReferencedFromCompositeIdKeyProperty()
		{
			var cfg = TestConfigurationHelper.GetDefaultConfiguration();

			Assert.That(
				() => cfg.AddResource("NHibernate.Test.NHSpecificTest.GH1001.Mappings.hbm.xml", GetType().Assembly),
				Throws.Nothing);
		}
	}
}
