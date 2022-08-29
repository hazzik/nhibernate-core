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

namespace NHibernate.Test.TypesTest
{
	using System.Threading.Tasks;
	/// <summary>
	/// Summary description for StringClobTypeFixture.
	/// </summary>
	[TestFixture]
	public class StringClobTypeFixtureAsync : TypeFixtureBase
	{
		protected override string TypeName
		{
			get { return "StringClob"; }
		}

		[Test]
		public async Task ReadWriteAsync()
		{
			ISession s = OpenSession();
			StringClobClass b = new StringClobClass();
			b.StringClob = "foo/bar/baz";
			await (s.SaveAsync(b));
			await (s.FlushAsync());
			s.Close();

			s = OpenSession();
			b = (StringClobClass) await (s.LoadAsync(typeof(StringClobClass), b.Id));
			Assert.AreEqual("foo/bar/baz", b.StringClob);
			await (s.DeleteAsync(b));
			await (s.FlushAsync());
			s.Close();
		}

		[Test]
		public async Task LongStringAsync()
		{
			string longString = new string('x', 10000);
			using (ISession s = OpenSession())
			{
				StringClobClass b = new StringClobClass();
				b.StringClob = longString;

				await (s.SaveAsync(b));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				StringClobClass b = (StringClobClass) await (s.CreateCriteria(
														  typeof(StringClobClass)).UniqueResultAsync());
				Assert.AreEqual(longString, b.StringClob);
				await (s.DeleteAsync(b));
				await (s.FlushAsync());
			}
		}

		[Test]
		public async Task InsertNullValueAsync()
		{
			using (ISession s = OpenSession())
			{
				StringClobClass b = new StringClobClass();
				b.StringClob = null;
				await (s.SaveAsync(b));
				await (s.FlushAsync());
			}

			using (ISession s = OpenSession())
			{
				StringClobClass b = (StringClobClass) await (s.CreateCriteria(
														  typeof(StringClobClass)).UniqueResultAsync());
				Assert.IsNull(b.StringClob);
				await (s.DeleteAsync(b));
				await (s.FlushAsync());
			}
		}
	}
}
