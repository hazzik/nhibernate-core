﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Cfg;
using NHibernate.Dialect.Function;
using NHibernate.DomainModel;
using NUnit.Framework;
using Environment = NHibernate.Cfg.Environment;

namespace NHibernate.Test.QueryTest
{
	using System.Threading.Tasks;
	[TestFixture]
	public class CountFixtureAsync
	{
		[Test]
		public async Task DefaultAsync()
		{
			Configuration cfg = TestConfigurationHelper.GetDefaultConfiguration();
			cfg.AddResource("NHibernate.DomainModel.Simple.hbm.xml", typeof(Simple).Assembly);
			cfg.SetProperty(Environment.Hbm2ddlAuto, "create-drop");
			ISessionFactory sf = cfg.BuildSessionFactory();

			using (ISession s = sf.OpenSession())
			{
				object count = await (s.CreateQuery("select count(*) from Simple").UniqueResultAsync());
				Assert.IsTrue(count is Int64);
			}
			await (sf.CloseAsync());
		}

		[Test]
		public async Task OverriddenAsync()
		{
			Configuration cfg = TestConfigurationHelper.GetDefaultConfiguration();
			cfg.SetProperty(Environment.Hbm2ddlAuto, "create-drop");
			cfg.AddResource("NHibernate.DomainModel.Simple.hbm.xml", typeof(Simple).Assembly);
			cfg.AddSqlFunction("count", new ClassicCountFunction());

			ISessionFactory sf = cfg.BuildSessionFactory();

			using (ISession s = sf.OpenSession())
			{
				object count = await (s.CreateQuery("select count(*) from Simple").UniqueResultAsync());
				Assert.IsTrue(count is Int32);
			}
			await (sf.CloseAsync());
		}
	}
}
