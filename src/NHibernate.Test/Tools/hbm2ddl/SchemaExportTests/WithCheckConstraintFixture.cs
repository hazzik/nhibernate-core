using NHibernate.Dialect;
using NHibernate.Exceptions;
using NUnit.Framework;

namespace NHibernate.Test.Tools.hbm2ddl.SchemaExportTests
{
	/// <summary>
	/// Shows that a mapped check constraint reaches the database and rejects an invalid row.
	/// </summary>
	[TestFixture]
	public class WithCheckConstraintFixture : TestCase
	{
		protected override string MappingsAssembly => "NHibernate.Test";

		protected override string[] Mappings =>
			new[] { "Tools.hbm2ddl.SchemaExportTests.WithCheckConstraint.hbm.xml" };

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			// The mapping uses check constraints, which Ms SQL CE does not support.
			return !(dialect is MsSqlCeDialect);
		}

		[Test]
		public void ColumnCheckRejectsInvalidRow()
		{
			Assume.That(Dialect.SupportsColumnCheck, "The dialect does not support column check constraints.");

			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Quantity breaks the column check, Price does not break the table check.
			session.Save(new WithCheckConstraint { Id = 1, Quantity = 0, Price = 10 });

			Assert.Throws<GenericADOException>(session.Flush);
		}

		[Test]
		public void TableCheckRejectsInvalidRow()
		{
			Assume.That(Dialect.SupportsTableCheck, "The dialect does not support table check constraints.");

			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// Price breaks the table check, Quantity does not break the column check.
			session.Save(new WithCheckConstraint { Id = 2, Quantity = 5, Price = 1 });

			Assert.Throws<GenericADOException>(session.Flush);
		}

		[Test]
		public void ChecksAcceptValidRow()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new WithCheckConstraint { Id = 3, Quantity = 5, Price = 10 });

			Assert.DoesNotThrow(session.Flush);
		}
	}
}
