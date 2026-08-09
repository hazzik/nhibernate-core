using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1294
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private int _rootEntityId;
		private int _targetId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<RootEntity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.ManyToOne(x => x.BaseJoin, m => m.Column("BaseJoinId"));
			});

			mapper.Class<BaseJoin>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
			});

			mapper.JoinedSubclass<SpecificJoin>(rc =>
			{
				rc.Bag(
					x => x.Problematic,
					m =>
					{
						m.Table("SpecificJoin_Target");
						m.Key(k => k.Column("SpecificJoinId"));
					},
					r => r.ManyToMany(mm => mm.Column("TargetId")));
			});

			mapper.JoinedSubclass<SiblingJoin>(rc =>
			{
				rc.Bag(
					x => x.Problematic,
					m =>
					{
						m.Table("SiblingJoin_Target");
						m.Key(k => k.Column("SiblingJoinId"));
					},
					r => r.ManyToMany(mm => mm.Column("TargetId")));
			});

			mapper.Class<Target>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var target = new Target();
			session.Save(target);

			var specificJoin = new SpecificJoin();
			specificJoin.Problematic.Add(target);
			session.Save(specificJoin);

			var root = new RootEntity { BaseJoin = specificJoin };
			session.Save(root);

			transaction.Commit();

			_rootEntityId = root.Id;
			_targetId = target.Id;
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateSQLQuery("delete from SpecificJoin_Target").ExecuteUpdate();
			session.CreateSQLQuery("delete from SiblingJoin_Target").ExecuteUpdate();
			session.CreateQuery("delete from System.Object").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void JoinAliasOnSubclassDoesNotJoinSiblingSubclassCollection()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			SpecificJoin specificJoinAlias = null;
			Target problematicAlias = null;

			var ids = session.QueryOver<RootEntity>()
				.Where(x => x.Id == _rootEntityId)
				.JoinAlias(x => x.BaseJoin, () => specificJoinAlias)
				.JoinAlias(() => specificJoinAlias.Problematic, () => problematicAlias)
				.Select(Projections.Property(() => problematicAlias.Id))
				.List<int>();

			Assert.That(ids, Has.Count.EqualTo(1));
			Assert.That(ids[0], Is.EqualTo(_targetId));
		}
	}
}
