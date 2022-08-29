﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH464
{
	using System.Threading.Tasks;
	/// <summary>
	/// This is a test class for composite-element with reflection optimizer
	/// </summary>
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		/// <summary>
		/// Mapping files used in the TestCase
		/// </summary>
		protected override string[] Mappings => new[] { "Promotion.hbm.xml" };

		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession session = OpenSession())
			using (ITransaction t = session.BeginTransaction())
			{
				session.Delete("from System.Object"); // clear everything from database
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction t = session.BeginTransaction())
			{
				session.Delete("from System.Object"); // clear everything from database
				t.Commit();
			}
			base.OnTearDown();
		}

		[Test]
		public async Task CompositeElementAsync()
		{
			Promotion promo = new Promotion();
			promo.Description = "test promo";
			promo.Window = new PromotionWindow();
			promo.Window.Dates.Add(new DateRange(DateTime.Today, DateTime.Today.AddDays(20)));

			int id = 0;
			using (ISession session = OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				id = (int) await (session.SaveAsync(promo));
				await (tx.CommitAsync());
			}

			using (ISession session = OpenSession())
			using (ITransaction tx = session.BeginTransaction())
			{
				promo = (Promotion) await (session.LoadAsync(typeof(Promotion), id));

				Assert.AreEqual(1, promo.Window.Dates.Count);
				Assert.AreEqual(DateTime.Today, ((DateRange) promo.Window.Dates[0]).Start);
				Assert.AreEqual(DateTime.Today.AddDays(20), ((DateRange) promo.Window.Dates[0]).End);

				await (tx.CommitAsync());
			}
		}
	}
}
