namespace NHibernate.Test.DialectTest
{
	using Dialect;
	using Mapping;
	using NUnit.Framework;

	[TestFixture]
	public class SqlCEDialectFixture
	{
        private MsSqlCeDialect dialect;

        [SetUp]
        public void SetUp()
        {
            dialect = new MsSqlCeDialect();
        }
        
        [Test]
		public void BinaryBlob_mapping_to_SqlCe_types()
		{
			SimpleValue sv = new SimpleValue();
			sv.TypeName = NHibernateUtil.BinaryBlob.Name;
			Column column = new Column();
			column.Value = sv;

			// no length, should produce maximum
			Assert.That(column.GetSqlType(dialect, null), Is.EqualTo("VARBINARY(8000)"));

			// maximum varbinary length is 8000
			column.Length = 8000;
			Assert.That(column.GetSqlType(dialect, null), Is.EqualTo("VARBINARY(8000)"));

			column.Length = 8001;
			Assert.That(column.GetSqlType(dialect, null), Is.EqualTo("IMAGE"));
		}
    
        [Test]
        public void QuotedSchemaNameWithSqlCE()
        {
            Table tbl = new Table();
            tbl.Schema = "`schema`";
            tbl.Name = "`name`";

            Assert.That(tbl.GetQualifiedName(dialect), Is.EqualTo("\"schema_name\""));
            Assert.That(dialect.Qualify("", "\"schema\"", "\"table\""), Is.EqualTo("\"schema_table\""));
        }

        [Test]
        public void QuotedTableNameWithoutSchemaWithSqlCE()
        {
            Table tbl = new Table();
            tbl.Name = "`name`";

            Assert.That(tbl.GetQualifiedName(dialect), Is.EqualTo("\"name\""));
        }

        [Test]
        public void QuotedSchemaNameWithUnqoutedTableInSqlCE()
        {
            Table tbl = new Table();
            tbl.Schema = "`schema`";
            tbl.Name = "name";

            Assert.That(tbl.GetQualifiedName(dialect), Is.EqualTo("\"schema_name\""));
            Assert.That(dialect.Qualify("", "\"schema\"", "table"), Is.EqualTo("\"schema_table\""));
        }

        [Test]
        public void QuotedCatalogSchemaNameWithSqlCE()
        {
            Table tbl = new Table();
            tbl.Catalog = "dbo";
            tbl.Schema = "`schema`";
            tbl.Name = "`name`";

            Assert.That(tbl.GetQualifiedName(dialect), Is.EqualTo("dbo.\"schema_name\""));
            Assert.That(dialect.Qualify("dbo", "\"schema\"", "\"table\""), Is.EqualTo("dbo.\"schema_table\""));
        }

        [Test]
        public void QuotedTableNameWithSqlCE()
        {
            Table tbl = new Table();
            tbl.Name = "`Group`";

            Assert.That(tbl.GetQualifiedName(dialect), Is.EqualTo("\"Group\""));
        }

        [Test]
        public void SchemaNameWithSqlCE()
        {
            Table tbl = new Table();
            tbl.Schema = "schema";
            tbl.Name = "name";

            Assert.That(tbl.GetQualifiedName(dialect), Is.EqualTo("schema_name"));
            Assert.That(dialect.Qualify("", "schema", "table"), Is.EqualTo("schema_table"));
        }
    }
}