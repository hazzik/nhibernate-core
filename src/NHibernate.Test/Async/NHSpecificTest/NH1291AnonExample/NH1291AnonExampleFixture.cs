﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using System.Collections.Generic;
using NHibernate.Criterion;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1291AnonExample
{
	using System.Threading.Tasks;
	[TestFixture]
	public class NH1291AnonExampleFixtureAsync : BugTestCase
	{
		protected override void OnTearDown()
		{
			base.OnTearDown();
			using (ISession session = OpenSession())
			{
				using (ITransaction tx = session.BeginTransaction())
				{
					session.Delete("from Person");
					session.Delete("from Home");
					tx.Commit();
				}
			}
		}

		protected override void OnSetUp()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					Person e1 = new Person("Joe", 10, 9);
					Person e2 = new Person("Sally", 20, 8);
					Person e3 = new Person("Tim", 20, 7); //20
					Person e4 = new Person("Fred", 40, 40);
					Person e5 = new Person("Fred", 50, 50);
					s.Save(e1);
					s.Save(e2);
					s.Save(e3);
					s.Save(e4);
					s.Save(e5);
					tx.Commit();
				}
			}
		}

		protected class PersonIQAnon
		{
			private int _iq;
			public PersonIQAnon(int iq)
			{
				IQ = iq;
			}

			public int IQ
			{
				get { return _iq; }
				set { _iq = value; }
			}
		}

		[Test]
		public async Task CanCreateAnonExampleForIntAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IList list = await (s.CreateCriteria(typeof(Person))
						.Add(Example.Create(new PersonIQAnon(40))).ListAsync());
					//c# 3.5: Example.Create( new { IQ = 40 } )
					Assert.AreEqual(1, list.Count);
					Assert.AreEqual("Fred", ((Person) list[0]).Name);
					await (tx.CommitAsync());
				}
			}
		}

		protected class PersonNameAnon
		{
			private string name;

			public PersonNameAnon(string name)
			{
				Name = name;
			}

			virtual public string Name
			{
				get { return name; }
				set { name = value; }
			}
		}

		[Test]
		public async Task CanCreateAnonExampleForStringLikeCompareAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IList list = await (s.CreateCriteria(typeof(Person))
						.Add(Example.Create(new PersonNameAnon("%all%")).EnableLike()).ListAsync());
					//c# 3.5: Example.Create( new { Name = "%all%" } )
					Assert.AreEqual(1, list.Count);
					Assert.AreEqual("Sally", ((Person) list[0]).Name);
					await (tx.CommitAsync());
				}
			}
		}

		[Test]
		public async Task CanQueryUsingSavedRelationsAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IList<Person> people = await (s.CreateCriteria(typeof(Person)).ListAsync<Person>());
					Home h1 = new Home("Eugene", 97402);
					Home h2 = new Home("Klamath Falls", 97603);
					people[0].Home = h1;
					people[1].Home = h2;
					await (s.SaveAsync(h1));
					await (s.SaveAsync(h2));

					IList list = await (s.CreateCriteria(typeof(Person)).CreateCriteria("Home")
						.Add(Example.Create(h1)).ListAsync());

					Assert.AreEqual(1, list.Count);
					Assert.AreEqual("Joe", ((Person) list[0]).Name);
					await (tx.CommitAsync());
				}
			}
		}

		protected class HomeAnon
		{
			private int zip;

			public HomeAnon(int zip)
			{
				Zip = zip;
			}

			virtual public int Zip
			{
				get { return zip; }
				set { zip = value; }
			}
		}

		[Test]
		public async Task CanQueryUsingAnonRelationsAsync()
		{
			using (ISession s = OpenSession())
			{
				using (ITransaction tx = s.BeginTransaction())
				{
					IList<Person> people = await (s.CreateCriteria(typeof(Person)).ListAsync<Person>());
					Home h1 = new Home("Eugene", 97402);
					Home h2 = new Home("Klamath Falls", 97603);
					people[0].Home = h1;
					people[1].Home = h2;
					await (s.SaveAsync(h1));
					await (s.SaveAsync(h2));

					IList list = await (s.CreateCriteria(typeof(Person))
						.CreateCriteria("Home").Add(Example.Create(new HomeAnon(97402))).ListAsync());
					//c# 3.5: Example.Create( new { Zip = 97402 } )

					Assert.AreEqual(1, list.Count);
					Assert.AreEqual("Joe", ((Person) list[0]).Name);
					await (tx.CommitAsync());
				}
			}
		}
	}
}
