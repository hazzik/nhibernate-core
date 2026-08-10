using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1213
{
	// NH-2143 - Order by doesn't clear in subselect if there is a projection type order bys
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Parent>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Weight);
					rc.Property(
						x => x.SortKey,
						m =>
						{
							m.Formula("(Weight)");
							m.Access(Accessor.Property);
						});
					rc.Bag(
						x => x.Children,
						m =>
						{
							m.Key(k => k.Column("ParentId"));
							m.Fetch(CollectionFetchMode.Subselect);
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<Child>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.ManyToOne(
						x => x.Parent,
						m =>
						{
							m.Column("ParentId");
							m.Insert(false);
							m.Update(false);
						});
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parent1 = new Parent { Weight = 1 };
			parent1.Children.Add(new Child { Parent = parent1 });
			var parent2 = new Parent { Weight = 2 };
			parent2.Children.Add(new Child { Parent = parent2 });

			session.Save(parent1);
			session.Save(parent2);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Child").ExecuteUpdate();
			session.CreateQuery("delete from Parent").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void SubselectFetchDoesNotRetainOrderByOnProjectionOrder()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var parents = session.Query<Parent>().OrderBy(x => x.SortKey).ToList();

			Assert.That(parents, Has.Count.EqualTo(2));

			using var spy = new SqlLogSpy();

			// Accessing the first parent's lazy collection triggers the subselect
			// fetch of the collections for every parent loaded by the previous query.
			var childrenCount = parents[0].Children.Count;

			Assert.That(childrenCount, Is.EqualTo(1));

			var subselectSql = spy.GetWholeLog();

			Assert.That(
				subselectSql,
				Does.Not.Contain("order by").IgnoreCase,
				"The subselect used for fetching the collection should not retain the outer query's ORDER BY clause: " + subselectSql);
		}
	}
}
