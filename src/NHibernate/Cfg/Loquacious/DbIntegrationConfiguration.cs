using System;
using System.Data;
using System.Data.Common;
using NHibernate.Connection;
using NHibernate.Driver;
using NHibernate.AdoNet;
using NHibernate.Exceptions;
using NHibernate.Transaction;

namespace NHibernate.Cfg.Loquacious
{
	public class DbIntegrationConfiguration 
	{
		private readonly Configuration configuration;

		public DbIntegrationConfiguration(Configuration configuration)
		{
			this.configuration = configuration;
			Connected = new ConnectionConfiguration(this);
			BatchingQueries = new BatcherConfiguration(this);
			Transactions = new TransactionConfiguration(this);
			CreateCommands = new CommandsConfiguration(this);
			Schema = new DbSchemaIntegrationConfiguration(this);
		}

		public Configuration Configuration => configuration;

		/// <summary>
		/// Define and configure the dialect to use.
		/// </summary>
		/// <typeparam name="TDialect">The dialect implementation inherited from <see cref="Dialect.Dialect"/>.</typeparam>
		/// <returns>The fluent configuration itself.</returns>
		public DbIntegrationConfiguration Using<TDialect>() where TDialect : Dialect.Dialect
		{
			configuration.SetProperty(Environment.Dialect, typeof(TDialect).AssemblyQualifiedName);
			return this;
		}

		public DbIntegrationConfiguration DisableKeywordsAutoImport()
		{
			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "none");
			return this;
		}

		public DbIntegrationConfiguration AutoQuoteKeywords()
		{
			configuration.SetProperty(Environment.Hbm2ddlKeyWords, "auto-quote");
			return this;
		}

		public DbIntegrationConfiguration LogSqlInConsole()
		{
			configuration.SetProperty(Environment.ShowSql, "true");
			return this;
		}

		public DbIntegrationConfiguration EnableLogFormattedSql()
		{
			configuration.SetProperty(Environment.FormatSql, "true");
			return this;
		}

		public ConnectionConfiguration Connected { get; }

		public BatcherConfiguration BatchingQueries { get; }

		public TransactionConfiguration Transactions { get; }

		public CommandsConfiguration CreateCommands { get; }

		public DbSchemaIntegrationConfiguration Schema { get; }
	}

	public class DbSchemaIntegrationConfiguration 
	{
		private readonly DbIntegrationConfiguration dbc;

		public DbSchemaIntegrationConfiguration(DbIntegrationConfiguration dbc)
		{
			this.dbc = dbc;
		}

		public DbIntegrationConfiguration Recreating()
		{
			dbc.Configuration.SetProperty(Environment.Hbm2ddlAuto, SchemaAutoAction.Recreate.ToString());
			return dbc;
		}

		public DbIntegrationConfiguration Creating()
		{
			dbc.Configuration.SetProperty(Environment.Hbm2ddlAuto, SchemaAutoAction.Create.ToString());
			return dbc;
		}

		public DbIntegrationConfiguration Updating()
		{
			dbc.Configuration.SetProperty(Environment.Hbm2ddlAuto, SchemaAutoAction.Update.ToString());
			return dbc;
		}

		public DbIntegrationConfiguration Validating()
		{
			dbc.Configuration.SetProperty(Environment.Hbm2ddlAuto, SchemaAutoAction.Validate.ToString());
			return dbc;
		}
	}

	public class CommandsConfiguration 
	{
		private readonly DbIntegrationConfiguration dbc;

		public CommandsConfiguration(DbIntegrationConfiguration dbc)
		{
			this.dbc = dbc;
		}

		public CommandsConfiguration Preparing()
		{
			dbc.Configuration.SetProperty(Environment.PrepareSql, "true");
			return this;
		}

		public CommandsConfiguration WithTimeout(byte seconds)
		{
			dbc.Configuration.SetProperty(Environment.CommandTimeout, seconds.ToString());
			return this;
		}

		public CommandsConfiguration ConvertingExceptionsThrough<TExceptionConverter>()
			where TExceptionConverter : ISQLExceptionConverter
		{
			dbc.Configuration.SetProperty(Environment.SqlExceptionConverter, typeof(TExceptionConverter).AssemblyQualifiedName);
			return this;
		}

		public CommandsConfiguration AutoCommentingSql()
		{
			dbc.Configuration.SetProperty(Environment.UseSqlComments, "true");
			return this;
		}

		public DbIntegrationConfiguration WithHqlToSqlSubstitutions(string csvQuerySubstitutions)
		{
			dbc.Configuration.SetProperty(Environment.QuerySubstitutions, csvQuerySubstitutions);
			return dbc;
		}

		public DbIntegrationConfiguration WithDefaultHqlToSqlSubstitutions()
		{
			return dbc;
		}

		/// <summary>
		/// Maximum depth of outer join fetching
		/// </summary>
		/// <remarks>
		/// 0 (zero) disable the usage of OuterJoinFetching
		/// </remarks>
		public CommandsConfiguration WithMaximumDepthOfOuterJoinFetching(byte maxFetchDepth)
		{
			dbc.Configuration.SetProperty(Environment.MaxFetchDepth, maxFetchDepth.ToString());
			return this;
		}
	}

	public class TransactionConfiguration 
	{
		private readonly DbIntegrationConfiguration dbc;

		public TransactionConfiguration(DbIntegrationConfiguration dbc)
		{
			this.dbc = dbc;
		}

		public DbIntegrationConfiguration Through<TFactory>() where TFactory : ITransactionFactory
		{
			dbc.Configuration.SetProperty(Environment.TransactionStrategy, typeof(TFactory).AssemblyQualifiedName);
			return dbc;
		}
	}

	public class BatcherConfiguration
	{
		private readonly DbIntegrationConfiguration dbc;

		public BatcherConfiguration(DbIntegrationConfiguration dbc)
		{
			this.dbc = dbc;
		}

		public BatcherConfiguration Through<TBatcher>() where TBatcher : IBatcherFactory
		{
			dbc.Configuration.SetProperty(Environment.BatchStrategy, typeof(TBatcher).AssemblyQualifiedName);
			return this;
		}

		public DbIntegrationConfiguration Each(short batchSize)
		{
			dbc.Configuration.SetProperty(Environment.BatchSize, batchSize.ToString());
			return dbc;
		}

		public BatcherConfiguration OrderingInserts()
		{
			dbc.Configuration.SetProperty(Environment.OrderInserts, true.ToString().ToLowerInvariant());
			return this;
		}

		public BatcherConfiguration DisablingInsertsOrdering()
		{
			dbc.Configuration.SetProperty(Environment.OrderInserts, false.ToString().ToLowerInvariant());
			return this;
		}
	}

	public class ConnectionConfiguration 
	{
		private readonly DbIntegrationConfiguration dbc;

		public ConnectionConfiguration(DbIntegrationConfiguration dbc)
		{
			this.dbc = dbc;
		}

		public ConnectionConfiguration Through<TProvider>() where TProvider : IConnectionProvider
		{
			dbc.Configuration.SetProperty(Environment.ConnectionProvider, typeof(TProvider).AssemblyQualifiedName);
			return this;
		}

		public ConnectionConfiguration By<TDriver>() where TDriver : IDriver
		{
			dbc.Configuration.SetProperty(Environment.ConnectionDriver, typeof(TDriver).AssemblyQualifiedName);
			return this;
		}

		public ConnectionConfiguration With(IsolationLevel level)
		{
			dbc.Configuration.SetProperty(Environment.Isolation, level.ToString());
			return this;
		}

		public ConnectionConfiguration Releasing(ConnectionReleaseMode releaseMode)
		{
			dbc.Configuration.SetProperty(Environment.ReleaseConnections, ConnectionReleaseModeParser.ToString(releaseMode));
			return this;
		}

		public DbIntegrationConfiguration Using(string connectionString)
		{
			dbc.Configuration.SetProperty(Environment.ConnectionString, connectionString);
			return dbc;
		}

		public DbIntegrationConfiguration Using(DbConnectionStringBuilder connectionStringBuilder)
		{
			dbc.Configuration.SetProperty(Environment.ConnectionString, connectionStringBuilder.ConnectionString);
			return dbc;
		}

		public DbIntegrationConfiguration ByAppConfing(string connectionStringName)
		{
			dbc.Configuration.SetProperty(Environment.ConnectionStringName, connectionStringName);
			return dbc;
		}
	}
}
