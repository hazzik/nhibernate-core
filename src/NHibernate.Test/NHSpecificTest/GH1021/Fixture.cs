using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1021
{
	using Cfg;

	[TestFixture]
	public class Fixture
	{
		// NH-3309: the dialect-scope element for database-object must contain the full name of a
		// class (a Dialect subclass). Reading the mapping (or building the session factory) should
		// raise an exception when the value is not valid as a class name, instead of silently
		// accepting it and letting the auxiliary database object never apply to any dialect.
		private const string Hbm = @"<?xml version='1.0' encoding='utf-8' ?>
<hibernate-mapping xmlns='urn:nhibernate-mapping-2.2'
					namespace='NHibernate.Test.NHSpecificTest.GH1021'
					assembly='NHibernate.Test'>
	<database-object>
		<create>create table gh1021_dummy (id int)</create>
		<drop>drop table gh1021_dummy</drop>
		<dialect-scope name='this is not a valid class name!!!'/>
	</database-object>
</hibernate-mapping>";

		[Test]
		public void InvalidDialectScopeNameShouldBeRejected()
		{
			var cfg = TestConfigurationHelper.GetDefaultConfiguration();
			cfg.AddXmlString(Hbm);

			ISessionFactory factory = null;
			try
			{
				Assert.That(
					() => factory = cfg.BuildSessionFactory(),
					Throws.TypeOf<MappingException>());
			}
			finally
			{
				factory?.Dispose();
			}
		}
	}
}
