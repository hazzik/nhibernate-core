using System.Threading;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Event;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1340
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.BatchSize, "0");
			configuration.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[]
			{
				new AuditEventListener()
			};
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Entity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.DynamicUpdate(true);
					rc.Property(x => x.Name);
					rc.Property(x => x.ModifiedByUser);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new Entity { Id = 1, Name = "old_name", ModifiedByUser = "nobody" });
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from Entity").ExecuteUpdate();
				transaction.Commit();
			}
		}

		[Test]
		public void OnPreUpdateChangeToNonDirtyPropertyIsPersistedWithDynamicUpdate()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = session.Get<Entity>(1);
				// Only Name is actually modified by the application code; ModifiedByUser
				// is only touched by the OnPreUpdate listener below.
				entity.Name = "new_name";
				transaction.Commit();
			}

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = session.Get<Entity>(1);
				Assert.That(entity.ModifiedByUser, Is.EqualTo("audited"),
					"The change made to ModifiedByUser inside OnPreUpdate must be persisted even though the property was not otherwise dirty.");
				transaction.Commit();
			}
		}

		public class AuditEventListener : IPreUpdateEventListener
		{
			public bool OnPreUpdate(PreUpdateEvent @event)
			{
				var index = @event.Persister.EntityMetamodel.GetPropertyIndex(nameof(Entity.ModifiedByUser));
				@event.State[index] = "audited";
				return false;
			}

			public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
			{
				return Task.FromResult(OnPreUpdate(@event));
			}
		}
	}
}
