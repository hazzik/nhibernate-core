﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using System.Reflection;

using NHibernate.Cfg;

using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH952
{
	using System.Threading.Tasks;
	[TestFixture]
	public class NH952FixtureAsync
	{
		private static readonly Assembly MyAssembly = typeof(NH952FixtureAsync).Assembly;
		private static readonly string MyNamespace = typeof(NH952FixtureAsync).Namespace;

		private static readonly string[] Resources = new string[]
			{
				// Order is important!
				MyNamespace + ".Asset.hbm.xml",
				MyNamespace + ".SellableItem.hbm.xml",
				MyNamespace + ".PhysicalItem.hbm.xml",
				MyNamespace + ".Item.hbm.xml"
			};

		[Test]
		public async Task OrderingAddResourcesAsync()
		{
			Configuration cfg = new Configuration();
			foreach (string res in Resources)
			{
				cfg.AddResource(res, MyAssembly);
			}
			await (cfg.BuildSessionFactory().CloseAsync());
		}
	}
}
