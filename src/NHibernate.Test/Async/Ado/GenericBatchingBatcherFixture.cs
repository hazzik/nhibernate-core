﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Diagnostics;
using System.Linq;
using NHibernate.AdoNet;
using NHibernate.Cfg;
using NHibernate.Dialect;
using NHibernate.Linq;
using NHibernate.SqlCommand;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.Ado
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class GenericBatchingBatcherFixtureAsync : TestCase
	{
		protected override string MappingsAssembly => "NHibernate.Test";

		protected override string[] Mappings => new[] { "Ado.VerySimple.hbm.xml" };

		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.BatchStrategy, typeof(GenericBatchingBatcherFactory).AssemblyQualifiedName);
			configuration.SetProperty(Environment.GenerateStatistics, "true");
			configuration.SetProperty(Environment.BatchSize, "1000");
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return !(dialect is FirebirdDialect) &&
				   !(dialect is Oracle8iDialect) &&
				   !(dialect is MsSqlCeDialect) &&
				   !(dialect is HanaDialectBase) &&
				   // A workaround exists for SQL Anywhere, see https://stackoverflow.com/a/32860293/1178314
				   // It would imply some tweaking in the generic batcher. The same workaround could
				   // be used for enabling future support.
				   !(dialect is SybaseSQLAnywhere10Dialect);
		}

		[Test]
		public async Task MassiveInsertUpdateDeleteTestAsync()
		{
			var totalRecords = 1000;
			await (BatchInsertAsync(totalRecords));
			await (BatchUpdateAsync(totalRecords));
			await (BatchDeleteAsync(totalRecords));

			await (DbShoudBeEmptyAsync());
		}

		[Test]
		public async Task BatchSizeTestAsync()
		{
			using (var sqlLog = new SqlLogSpy())
			using (var s = Sfi.OpenSession())
			using (var tx = s.BeginTransaction())
			{
				s.SetBatchSize(5);
				for (var i = 0; i < 20; i++)
				{
					await (s.SaveAsync(new VerySimple { Id = 1 + i, Name = $"Fabio{i}", Weight = 1.45 + i }));
				}
				await (tx.CommitAsync());

				var log = sqlLog.GetWholeLog();
				Assert.That(FindAllOccurrences(log, "Batch commands:"), Is.EqualTo(4));
			}
			await (CleanupAsync());
		}

		// Demonstrates a 50% performance gain with SQL-Server, around 40% for PostgreSQL,
		// around 15% for MySql, but around 200% performance loss for SQLite.
		// (Tested with databases on same machine for all cases.)
		[Theory, Explicit("This is a performance test, to be checked manually.")]
		public async Task MassivePerformanceTestAsync(bool batched)
		{
			if (batched)
			{
				// Bring down batch size to a reasonnable value, otherwise performances are worsen.
				cfg.SetProperty(Environment.BatchSize, "50");
			}
			else
			{
				cfg.SetProperty(Environment.BatchStrategy, typeof(NonBatchingBatcherFactory).AssemblyQualifiedName);
				cfg.Properties.Remove(Environment.BatchSize);
			}
			RebuildSessionFactory();

			try
			{
				// Warm up
				await (MassiveInsertUpdateDeleteTestAsync());

				var chrono = new Stopwatch();
				chrono.Start();
				await (MassiveInsertUpdateDeleteTestAsync());
				Console.WriteLine($"Elapsed time: {chrono.Elapsed}");
			}
			finally
			{
				Configure(cfg);
				RebuildSessionFactory();
			}
		}

		[Test]
		public async Task InterceptorOnPrepareStatementTestAsync()
		{
			var interceptor = new DatabaseInterceptor();
			using (var sqlLog = new SqlLogSpy())
			using (var s = Sfi.WithOptions().Interceptor(interceptor).OpenSession())
			using (var tx = s.BeginTransaction())
			{
				s.SetBatchSize(5);
				for (var i = 0; i < 20; i++)
				{
					await (s.SaveAsync(new VerySimple { Id = 1 + i, Name = $"Fabio{i}", Weight = 1.45 + i }));
				}

				await (tx.CommitAsync());

				Assert.That(interceptor.TotalCalls, Is.EqualTo(1));
				var log = sqlLog.GetWholeLog();
				Assert.That(FindAllOccurrences(log, "/* TEST */"), Is.EqualTo(20));
			}

			interceptor = new DatabaseInterceptor();
			using (var sqlLog = new SqlLogSpy())
			using (var s = Sfi.WithOptions().Interceptor(interceptor).OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var future = s.Query<VerySimple>().ToFuture();
				s.Query<VerySimple>().Where(o => o.Weight > 0).ToFuture();

				using (var enumerator = (await (future.GetEnumerableAsync())).GetEnumerator())
				{
					while (enumerator.MoveNext()) { }
				}

				await (tx.CommitAsync());

				var totalCalls = Sfi.ConnectionProvider.Driver.SupportsMultipleQueries ? 1 : 2;
				Assert.That(interceptor.TotalCalls, Is.EqualTo(totalCalls));
				var log = sqlLog.GetWholeLog();
				Assert.That(FindAllOccurrences(log, "/* TEST */"), Is.EqualTo(totalCalls));
			}

			await (CleanupAsync());
		}

		private class DatabaseInterceptor : EmptyInterceptor
		{
			public int TotalCalls { get; private set; }

			public override SqlString OnPrepareStatement(SqlString sql)
			{
				TotalCalls++;
				return sql.Append("/* TEST */");
			}
		}

		private async Task BatchInsertAsync(int totalRecords, CancellationToken cancellationToken = default(CancellationToken))
		{
			Sfi.Statistics.Clear();
			using (var s = Sfi.OpenSession())
			using (var tx = s.BeginTransaction())
			{
				for (var i = 0; i < totalRecords; i++)
				{
					await (s.SaveAsync(new VerySimple { Id = 1 + i, Name = $"Fabio{i}", Weight = 1.45 + i }, cancellationToken));
				}
				await (tx.CommitAsync(cancellationToken));
			}
			Assert.That(Sfi.Statistics.PrepareStatementCount, Is.EqualTo(1));
		}

		public async Task BatchUpdateAsync(int totalRecords, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var s = Sfi.OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var items = await (s.Query<VerySimple>().ToListAsync(cancellationToken));
				Assert.That(items.Count, Is.EqualTo(totalRecords));

				foreach (var item in items)
				{
					item.Weight += 5;
					await (s.UpdateAsync(item, cancellationToken));
				}

				Sfi.Statistics.Clear();
				await (tx.CommitAsync(cancellationToken));
			}
			Assert.That(Sfi.Statistics.PrepareStatementCount, Is.EqualTo(1));
		}

		public async Task BatchDeleteAsync(int totalRecords, CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var s = Sfi.OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var items = await (s.Query<VerySimple>().ToListAsync(cancellationToken));
				Assert.That(items.Count, Is.EqualTo(totalRecords));

				foreach (var item in items)
				{
					await (s.DeleteAsync(item, cancellationToken));
				}

				Sfi.Statistics.Clear();
				await (tx.CommitAsync(cancellationToken));
			}
			Assert.That(Sfi.Statistics.PrepareStatementCount, Is.EqualTo(1));
		}

		private async Task DbShoudBeEmptyAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var s = Sfi.OpenSession())
			using (var tx = s.BeginTransaction())
			{
				var items = await (s.Query<VerySimple>().ToListAsync(cancellationToken));
				Assert.That(items.Count, Is.EqualTo(0));

				await (tx.CommitAsync(cancellationToken));
			}
		}

		private async Task CleanupAsync(CancellationToken cancellationToken = default(CancellationToken))
		{
			using (var s = Sfi.OpenSession())
			using (var t = s.BeginTransaction())
			{
				await (s.CreateQuery("delete from VerySimple").ExecuteUpdateAsync(cancellationToken));
				await (t.CommitAsync(cancellationToken));
			}
		}

		private int FindAllOccurrences(string source, string substring)
		{
			if (source == null)
			{
				return 0;
			}
			int n = 0, count = 0;
			while ((n = source.IndexOf(substring, n, StringComparison.InvariantCulture)) != -1)
			{
				n += substring.Length;
				++count;
			}
			return count;
		}
	}
}
