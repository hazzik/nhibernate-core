﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Data;
using System.Data.Common;
using NHibernate.Dialect;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Util;
using NUnit.Framework;

namespace NHibernate.Test.ExceptionsTest
{
	using System.Threading.Tasks;
	[TestFixture]
	public class SQLExceptionConversionTestAsync : TestCase
	{
		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override string[] Mappings
		{
			get { return new[] { "ExceptionsTest.User.hbm.xml", "ExceptionsTest.Group.hbm.xml" }; }
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return dialect is MsSql2000Dialect || dialect is PostgreSQLDialect || dialect is FirebirdDialect || dialect is Oracle8iDialect;
		}

		protected override bool AppliesTo(ISessionFactoryImplementor factory)
		{
			var driver = factory.ConnectionProvider.Driver;
			return !(driver is OracleDataClientDriver) && !(driver is OracleManagedDataClientDriver) && !(driver is OracleLiteDataClientDriver) && !(driver is OdbcDriver) && !(driver is OleDbDriver);
		}

		protected override void Configure(Cfg.Configuration configuration)
		{
			if (Dialect is MsSql2000Dialect)
			{
				configuration.SetProperty(
					Cfg.Environment.SqlExceptionConverter,
					typeof(MSSQLExceptionConverterExample).AssemblyQualifiedName);
			}

			if (Dialect is Oracle8iDialect)
			{
				configuration.SetProperty(
					Cfg.Environment.SqlExceptionConverter,
					typeof(OracleClientExceptionConverterExample).AssemblyQualifiedName);
			}

			if (Dialect is PostgreSQLDialect)
			{
				configuration.SetProperty(
					Cfg.Environment.SqlExceptionConverter,
					typeof(PostgresExceptionConverterExample).AssemblyQualifiedName);
			}

			if (Dialect is FirebirdDialect)
			{
				configuration.SetProperty(
					Cfg.Environment.SqlExceptionConverter,
					typeof(FbExceptionConverterExample).AssemblyQualifiedName);
			}
		}

		[Test]
		public async Task IntegrityViolationAsync()
		{
			//ISQLExceptionConverter converter = Dialect.BuildSQLExceptionConverter();
			ISQLExceptionConverter converter = Sfi.Settings.SqlExceptionConverter;

			using (var session = OpenSession())
			using (var tran = session.BeginTransaction())
			{
				var connection = session.Connection;

				// Attempt to insert some bad values into the T_MEMBERSHIP table that should
				// result in a constraint violation
				DbCommand ps = null;
				try
				{
					ps = connection.CreateCommand();
					ps.CommandType = CommandType.Text;
					ps.CommandText = "INSERT INTO T_MEMBERSHIP (user_id, group_id) VALUES (@p1, @p2)";
					var pr = ps.CreateParameter();
					pr.ParameterName = "p1";
					pr.DbType = DbType.Int64;
					pr.Value = 52134241L; // Non-existent user_id
					ps.Parameters.Add(pr);

					pr = ps.CreateParameter();
					pr.ParameterName = "p2";
					pr.DbType = DbType.Int64;
					pr.Value = 5342L; // Non-existent group_id
					ps.Parameters.Add(pr);

					tran.Enlist(ps);
					await (ps.ExecuteNonQueryAsync());

					Assert.Fail("INSERT should have failed");
				}
				catch (Exception sqle)
				{
					ADOExceptionReporter.LogExceptions(sqle, "Just output!!!!");
					Exception adoException = converter.Convert(new AdoExceptionContextInfo { SqlException = sqle });
					Assert.AreEqual(
						typeof(ConstraintViolationException),
						adoException.GetType(),
						"Bad conversion [" + sqle.Message + "]");
					ConstraintViolationException ex = (ConstraintViolationException) adoException;
					Console.WriteLine("Violated constraint name: " + ex.ConstraintName);
				}
				finally
				{
					if (ps != null)
					{
						try
						{
							ps.Dispose();
						}
						catch (Exception)
						{
							// ignore...
						}
					}
				}

				await (tran.RollbackAsync());
				session.Close();
			}
		}

		[Test]
		public async Task BadGrammarAsync()
		{
			//ISQLExceptionConverter converter = Dialect.BuildSQLExceptionConverter();
			ISQLExceptionConverter converter = Sfi.Settings.SqlExceptionConverter;

			ISession session = OpenSession();
			var connection = session.Connection;

			// prepare/execute a query against a non-existent table
			DbCommand ps = null;
			try
			{
				ps = connection.CreateCommand();
				ps.CommandType = CommandType.Text;
				ps.CommandText = "SELECT user_id, user_name FROM tbl_no_there";
				await (ps.ExecuteNonQueryAsync());

				Assert.Fail("SQL compilation should have failed");
			}
			catch (Exception sqle)
			{
				Assert.AreEqual(typeof(SQLGrammarException),
								converter.Convert(new AdoExceptionContextInfo { SqlException = sqle }).GetType(),
								"Bad conversion [" + sqle.Message + "]");
			}
			finally
			{
				if (ps != null)
				{
					try
					{
						ps.Dispose();
					}
					catch (Exception)
					{
						// ignore...
					}
				}
			}

			session.Close();
		}
	}
}
