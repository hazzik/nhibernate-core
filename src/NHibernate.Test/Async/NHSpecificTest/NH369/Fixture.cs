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
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH369
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync
	{
		[Test]
		public async Task KeyManyToOneAndNormalizedPersisterAsync()
		{
			Configuration cfg = new Configuration();
			await (cfg
				.AddClass(typeof(BaseClass))
				.AddClass(typeof(KeyManyToOneClass))
				.BuildSessionFactory().CloseAsync());
		}
	}
}
