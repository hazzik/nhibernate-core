﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2614
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Save(new ConcreteClass1 { Name = "C1" });
				s.Save(new ConcreteClass2 { Name = "C2" });
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.CreateQuery("delete from System.Object").ExecuteUpdate();
				t.Commit();
			}
		}

		[Test]
		public async Task PolymorphicListReturnsCorrectResultsAsync()
		{
			using (var s = OpenSession())
			using (s.BeginTransaction())
			{
				var query = s.CreateQuery(
					@"SELECT Name FROM NHibernate.Test.NHSpecificTest.GH2614.BaseClass ROOT");
				query.SetMaxResults(5);
				var list = await (query.ListAsync());
				Assert.That(list.Count, Is.EqualTo(2));
			}
		}
	}
}
