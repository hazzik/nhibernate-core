﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2055
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override bool AppliesTo(NHibernate.Dialect.Dialect dialect)
		{
			return (dialect is Dialect.MsSql2000Dialect);
		}

		protected override void Configure(Configuration configuration)
		{
			base.Configure(configuration);
			cfg = configuration;
		}

		[Test]
		public async Task CanCreateAndDropSchemaAsync()
		{
			using (var s = Sfi.OpenSession())
			{
				using (var cmd = s.Connection.CreateCommand())
				{
					cmd.CommandType = CommandType.StoredProcedure;

					cmd.CommandText = "test_proc1";

					Assert.AreEqual(1, await (cmd.ExecuteScalarAsync()));

					cmd.CommandText = "test_proc2";

					Assert.AreEqual(2, await (cmd.ExecuteScalarAsync()));
				}
			}
		}
	}
}
