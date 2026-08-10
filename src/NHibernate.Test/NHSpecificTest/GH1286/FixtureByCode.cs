using System;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Event;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1286
{
	// NH-3276 / GH-1286: a property set by an IPreUpdateEventListener on a joined
	// subclass table is silently dropped when no property belonging to that same
	// table was already dirty before the listener ran.
	[TestFixture]
	public class ByCodeFixture : TestCaseMappingByCode
	{
		private const int ExampleId = 1;

		protected override void Configure(Configuration configuration)
		{
			configuration.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[]
			{
				new SetSubNameListener()
			};
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<BaseEntity>(rc =>
			{
				rc.Table("GH1286BaseEntity");
				rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
				rc.Property(x => x.Name);
			});
			mapper.JoinedSubclass<SubEntity>(rc =>
			{
				rc.Table("GH1286SubEntity");
				rc.Key(k => k.Column("Id"));
				rc.Property(x => x.SubName);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new SubEntity { Id = ExampleId, Name = "old_name", SubName = "old_sub_name" });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from System.Object").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ChangeSetByPreUpdateListenerOnJoinedSubclassIsPersisted()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = session.Get<SubEntity>(ExampleId);

				// Only a base-class (root table) property is changed by the caller. No
				// property of the joined subclass table is touched, so it is not part
				// of NHibernate's own dirty-property list. The listener nevertheless
				// sets SubName, which lives in the subclass table, and that change is
				// expected to reach the database.
				entity.Name = "new_name";

				transaction.Commit();
			}

			using (var session = OpenSession())
			{
				var entity = session.Get<SubEntity>(ExampleId);

				Assert.That(entity.SubName, Is.EqualTo("set_by_listener"),
					"The value set by the IPreUpdateEventListener on the joined subclass table was not persisted");
			}
		}

		public class SetSubNameListener : IPreUpdateEventListener
		{
			public bool OnPreUpdate(PreUpdateEvent @event)
			{
				if (@event.Entity is SubEntity)
				{
					var index = Array.IndexOf(@event.Persister.PropertyNames, nameof(SubEntity.SubName));
					@event.State[index] = "set_by_listener";
				}

				return false;
			}

			public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
			{
				return Task.FromResult(OnPreUpdate(@event));
			}
		}

		public class BaseEntity
		{
			public virtual int Id { get; set; }
			public virtual string Name { get; set; }
		}

		public class SubEntity : BaseEntity
		{
			public virtual string SubName { get; set; }
		}
	}
}
