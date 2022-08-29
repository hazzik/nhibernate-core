﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3414
{
	using System.Threading.Tasks;
	[TestFixture]
	public class ByCodeFixtureAsync : TestCaseMappingByCode
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			// Test ordering by a scalar sub-select.
			return TestDialect.SupportsAggregatingScalarSubSelectsInOrderBy;
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
				rc.Property(x => x.SomeValue);
				rc.ManyToOne(x => x.Parent, map => map.Column("ParentId"));
				rc.Bag(x => x.Children, map =>
				{
					map.Access(Accessor.NoSetter);
					map.Key(km => km.Column("ParentId"));
					map.Cascade(Mapping.ByCode.Cascade.All.Include(Mapping.ByCode.Cascade.DeleteOrphans));
				}, rel => rel.OneToMany());
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				var e1 = new Entity
				{
					Name = "A",
					SomeValue = 1,
				};
				e1.AddChild(new Entity { Name = "X", SomeValue = 3 });
				e1.AddChild(new Entity { Name = "Z", SomeValue = 10 });
				session.Save(e1);

				var e2 = new Entity
				{
					Name = "B",
					SomeValue = 2,
				};
				e2.AddChild(new Entity { Name = "Y", SomeValue = 10 });
				e2.AddChild(new Entity { Name = "Z", SomeValue = 2 });
				session.Save(e2);

				var e3 = new Entity
				{
					Name = "A",
					SomeValue = 3,
				};
				e3.AddChild(new Entity { Name = "X", SomeValue = 9 });
				e3.AddChild(new Entity { Name = "Y", SomeValue = 1 });
				session.Save(e3);

				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task OrderByNoQueriesTestAsync()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.Query<Entity>()
								.Where(x => x.Parent == null)
								.OrderBy(x => x.Name)   // Expression
								.ThenBy(x => x.SomeValue);  // Expression

				var results = await (query.ToListAsync());
				Assert.That(results.Select(x => x.ToString()), Is.EqualTo(new[] { "A1", "A3", "B2" }));
			}
		}

		[Test]
		public async Task OrderBySingleQueryTestAsync()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.Query<Entity>()
								.Where(x => x.Parent == null)
								// This could be any expression that generates a subquery
								.OrderBy(x => x.Children.Select(c => c.SomeValue).Min());

				var results = await (query.ToListAsync());
				Assert.That(results.Select(x => x.ToString()), Is.EqualTo(new[] { "A3", "B2", "A1" }));
			}
		}

		[Test]
		public async Task OrderBySingleQueryComplexAsync()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.Query<Entity>()
								.Where(x => x.Parent == null)
								.OrderBy(x => x.Name)   // Expression
														// This could be any expression that generates a subquery
								.ThenBy(x => x.Children.Select(c => c.SomeValue).Min());

				var results = await (query.ToListAsync());
				Assert.That(results.Select(x => x.ToString()), Is.EqualTo(new[] { "A3", "A1", "B2" }));
			}
		}

		[Test]
		public async Task OrderBySingleQueryComplexReverseAsync()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.Query<Entity>()
								.Where(x => x.Parent == null)
								// This could be any expression that generates a subquery
								.OrderByDescending(x => x.Children.Select(c => c.SomeValue).Max())
								.ThenBy(x => x.Name);   // Expression

				var results = await (query.ToListAsync());
				Assert.That(results.Select(x => x.ToString()), Is.EqualTo(new[] { "A1", "B2", "A3" }));
			}
		}

		[Test]
		public async Task OrderByMultipleQueryAsync()
		{
			using (ISession session = OpenSession())
			using (session.BeginTransaction())
			{
				var query = session.Query<Entity>()
								.Where(x => x.Parent == null)
								// These could be any expressions that generate a subqueries
								.OrderByDescending(x => x.Children.Select(c => c.SomeValue).Max())
								.ThenBy(x => x.Children.Select(c => c.SomeValue).Min());

				var results = await (query.ToListAsync());
				Assert.That(results.Select(x => x.ToString()), Is.EqualTo(new[] { "B2", "A1", "A3" }));
			}
		}
	}
}
