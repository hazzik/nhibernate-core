using System.Collections;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Transform;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1317
{
	// NH-3538 / GH-1317: DistinctRootEntityResultTransformer only works properly for linear chains.
	// When a root entity is connected via AddJoin to two different associations (a non-linear,
	// branching fetch), the deduplication of the root entity is reported to break down.
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Root>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
					rc.Set(
						x => x.As,
						m =>
						{
							m.Key(k => k.Column("RootId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
					rc.Set(
						x => x.Bs,
						m =>
						{
							m.Key(k => k.Column("RootId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<ChildA>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
				});

			mapper.Class<ChildB>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var root = new Root { Name = "root" };
			root.As.Add(new ChildA { Name = "a1" });
			root.As.Add(new ChildA { Name = "a2" });
			root.Bs.Add(new ChildB { Name = "b1" });
			root.Bs.Add(new ChildB { Name = "b2" });
			session.Save(root);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from ChildA").ExecuteUpdate();
			session.CreateQuery("delete from ChildB").ExecuteUpdate();
			session.CreateQuery("delete from Root").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void AddJoinToTwoDifferentAssociationsProducesOneDistinctRoot()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			const string sql =
				"select {root.*}, {a.*}, {b.*} " +
				"from Root root " +
				"inner join ChildA a on root.Id = a.RootId " +
				"inner join ChildB b on root.Id = b.RootId";

			IList list = session.CreateSQLQuery(sql)
			                     .AddEntity("root", typeof(Root))
			                     .AddJoin("a", "root.As")
			                     .AddJoin("b", "root.Bs")
			                     .SetResultTransformer(new DistinctRootEntityResultTransformer())
			                     .List();

			transaction.Commit();

			// The underlying SQL produces a cross join of the two child collections (2 As x 2 Bs = 4 rows),
			// but there is only one distinct Root; DistinctRootEntityResultTransformer is expected to
			// collapse the 4 rows down to that single Root instance.
			Assert.That(list.Count, Is.EqualTo(1), "Expected the 4 joined rows to collapse to a single distinct root entity");

			var root = (Root) list[0];
			Assert.That(root.As.Count, Is.EqualTo(2), "Root.As should contain both fetched children");
			Assert.That(root.Bs.Count, Is.EqualTo(2), "Root.Bs should contain both fetched children");
		}
	}
}
