﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Criterion;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.CriteriaQueryOnComponentCollection
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : TestCase
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.FormatSql, "false");
		}

		protected override void OnSetUp()
		{
			using (var s = Sfi.OpenSession())
			using (var t = s.BeginTransaction())
			{
				var parent = new Employee
				{
					Id = 2,
				};
				var emp = new Employee
				{
					Id = 1,
					Amounts = new HashSet<Money>
					{
						new Money {Amount = 9, Currency = "USD"},
						new Money {Amount = 3, Currency = "EUR"},
					},
					ManagedEmployees = new HashSet<ManagedEmployee>
					{
						new ManagedEmployee
						{
							Position = "parent",
							Employee = parent
						}
					}
				};
				s.Save(parent);
				s.Save(emp);

				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var s = Sfi.OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Delete("from System.Object");

				t.Commit();
			}
		}

		[Test]
		public async Task CanQueryByCriteriaOnSetOfCompositeElementAsync()
		{
			using (var s = Sfi.OpenSession())
			{
				var list = await (s.CreateCriteria<Employee>()
							.CreateCriteria("ManagedEmployees")
							.Add(Restrictions.Eq("Position", "parent"))
							.SetResultTransformer(new RootEntityResultTransformer())
							.ListAsync());
				Assert.That(list, Has.Count.EqualTo(1));
				Assert.That(list[0], Is.Not.Null);
				Assert.That(list[0], Is.TypeOf<Employee>());
				Assert.That(((Employee) list[0]).Id, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task CanQueryByCriteriaOnSetOfElementAsync()
		{
			using (var s = Sfi.OpenSession())
			{
				var list = await (s.CreateCriteria<Employee>()
							.CreateCriteria("Amounts")
							.Add(Restrictions.Gt("Amount", 5m))
							.SetResultTransformer(new RootEntityResultTransformer())
							.ListAsync());
				Assert.That(list, Has.Count.EqualTo(1));
				Assert.That(list[0], Is.Not.Null);
				Assert.That(list[0], Is.TypeOf<Employee>());
				Assert.That(((Employee) list[0]).Id, Is.EqualTo(1));
			}
		}

		[TestCase(JoinType.LeftOuterJoin)]
		[TestCase(JoinType.InnerJoin)]
		public async Task CanQueryByCriteriaOnSetOfElementByCreateAliasAsync(JoinType joinType)
		{
			using (var s = Sfi.OpenSession())
			{
				var list = await (s.CreateCriteria<Employee>("x")
							.CreateAlias("x.Amounts", "amount", joinType)
							.Add(Restrictions.Gt("amount.Amount", 5m))
							.SetResultTransformer(new RootEntityResultTransformer())
							.ListAsync());
				Assert.That(list, Has.Count.EqualTo(1));
				Assert.That(list[0], Is.Not.Null);
				Assert.That(list[0], Is.TypeOf<Employee>());
				Assert.That(((Employee) list[0]).Id, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task CanQueryByCriteriaOnSetOfCompositeElement_UsingDetachedCriteriaAsync()
		{
			using (var s = Sfi.OpenSession())
			{
				var list = await (s.CreateCriteria<Employee>()
							.Add(Subqueries.PropertyIn("id",
													   DetachedCriteria.For<Employee>()
																	   .SetProjection(Projections.Id())
																	   .CreateCriteria("Amounts")
																	   .Add(Restrictions.Gt("Amount", 5m))))
							.ListAsync());
				Assert.That(list, Has.Count.EqualTo(1));
				Assert.That(list[0], Is.Not.Null);
				Assert.That(list[0], Is.TypeOf<Employee>());
				Assert.That(((Employee) list[0]).Id, Is.EqualTo(1));
			}
		}

		protected override string[] Mappings
		{
			get { return new[] { "NHSpecificTest.CriteriaQueryOnComponentCollection.Mappings.hbm.xml" }; }
		}

		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}
	}
}
