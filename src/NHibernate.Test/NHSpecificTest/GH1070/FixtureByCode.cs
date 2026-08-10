using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1070
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		private Guid _aId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<A>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
					rc.Bag(
						x => x.BCollection,
						m =>
						{
							m.Key(k => k.Column("AId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<B>(
				rc => rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb)));

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var a = new A();
			a.BCollection.Add(new B());
			a.BCollection.Add(new B());
			session.Save(a);
			_aId = a.Id;

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Delete("from System.Object");

			transaction.Commit();
		}

		// NH-2134 / GH-1070: "left join fetch" on a collection should still eagerly
		// initialize the collection of an already loaded owner entity, even when the
		// HQL select list only projects a scalar property of the owner (no owner data
		// itself is fetched into the result).
		[Test]
		public void LeftJoinFetchInitializesCollectionOfAlreadyLoadedOwner()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// The owner is already present in the session, with its collection not yet initialized.
			var a = session.Get<A>(_aId);
			Assert.That(NHibernateUtil.IsInitialized(a.BCollection), Is.False, "Precondition failed: collection already initialized");

			session.CreateQuery("select a.Id from A a left join fetch a.BCollection b").List<Guid>();

			Assert.That(NHibernateUtil.IsInitialized(a.BCollection), Is.True, "Collection was not initialized by the join fetch");
			Assert.That(a.BCollection, Has.Count.EqualTo(2));

			transaction.Commit();
		}
	}
}
