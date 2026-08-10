using System;
using System.IO;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.NHSpecificTest.GH1022
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			// show_sql alone (without also raising the "NHibernate.SQL" log4net logger to Debug,
			// which the test suite otherwise turns Off, see log4net.xml) must still print every
			// insert/update to the console, batched or not.
			configuration.SetProperty(Environment.ShowSql, "true");
			configuration.SetProperty(Environment.BatchSize, "10");
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(
				rc =>
				{
					// HiLo pre-allocates the identifier without needing the insert to report it back
					// (unlike an identity/native "returning id" strategy), so the insert can actually
					// go through the batcher instead of bypassing it.
					rc.Id(x => x.Id, m => m.Generator(Generators.HighLow, g => g.Params(new { max_lo = 100 })));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Entity").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void BatchedInsertIsWrittenToConsoleWhenShowSqlIsEnabled()
		{
			var originalOut = Console.Out;
			var capturedOut = new StringWriter();
			Console.SetOut(capturedOut);
			try
			{
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					session.Save(new Entity { Name = "original" });
					transaction.Commit();
				}
			}
			finally
			{
				Console.SetOut(originalOut);
			}

			var consoleOutput = capturedOut.ToString();
			Assert.That(
				consoleOutput,
				Does.Contain("insert").IgnoreCase,
				"The batched insert statement was not printed to the console even though show_sql is enabled.");
		}

		[Test]
		public void BatchedUpdateIsWrittenToConsoleWhenShowSqlIsEnabled()
		{
			int id;
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = new Entity { Name = "original" };
				session.Save(entity);
				transaction.Commit();
				id = entity.Id;
			}

			var originalOut = Console.Out;
			var capturedOut = new StringWriter();
			Console.SetOut(capturedOut);
			try
			{
				using (var session = OpenSession())
				using (var transaction = session.BeginTransaction())
				{
					var entity = session.Load<Entity>(id);
					entity.Name = "updated";
					transaction.Commit();
				}
			}
			finally
			{
				Console.SetOut(originalOut);
			}

			var consoleOutput = capturedOut.ToString();
			Assert.That(
				consoleOutput,
				Does.Contain("update").IgnoreCase,
				"The batched update statement was not printed to the console even though show_sql is enabled.");
		}
	}
}
