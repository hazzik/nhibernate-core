﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NUnit.Framework;

namespace NHibernate.Test.IdTest
{
	using System.Threading.Tasks;
	[TestFixture]
	public class HiLoTableGeneratorInt64FixtureAsync : IdFixtureBase
	{
		protected override string TypeName
		{
			get { return "HiLoInt64"; }
		}

		[Test]
		public async Task ReadWriteAsync()
		{
			Int64 id;
			ISession s = OpenSession();
			HiLoInt64Class b = new HiLoInt64Class();
			await (s.SaveAsync(b));
			await (s.FlushAsync());
			id = b.Id;
			s.Close();

			s = OpenSession();
			b = (HiLoInt64Class) await (s.LoadAsync(typeof(HiLoInt64Class), b.Id));
			Assert.AreEqual(id, b.Id);
			await (s.DeleteAsync(b));
			await (s.FlushAsync());
			s.Close();
		}
	}
}
