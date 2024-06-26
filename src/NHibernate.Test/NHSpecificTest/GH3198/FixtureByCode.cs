using System;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Event;
using NHibernate.Mapping.ByCode;
using NHibernate.Type;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3198
{
	[TestFixture]
	public partial class ByCodeFixture : TestCaseMappingByCode
	{
		private const int EXAMPLE_ID_VALUE = 1;

		protected override void Configure(Configuration configuration)
		{
			// A listener always returning true
			configuration.EventListeners.PreUpdateEventListeners = new IPreUpdateEventListener[]
			{
				new TestEventListener()
			};
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Table("Entity");
				rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
				rc.Property(x => x.Name, x => x.Type<StringType>());
				rc.Version(x => x.Version, vm => { });
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var e1 = new Entity { Id = EXAMPLE_ID_VALUE, Name = "old_name"  };
				session.Save(e1);
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.CreateQuery("delete from System.Object").ExecuteUpdate();

				transaction.Commit();
			}
		}

		[Test]
		public void TestVersionNotChangedWhenPreUpdateEventVetod()
		{
			using (var session = OpenSession())
			{
				var entity = session.Load<Entity>(EXAMPLE_ID_VALUE);

				entity.Name = "new_name";
				session.Update(entity);
				
				var versionBeforeFlush = entity.Version;
				
				session.Flush();

				var versionAfterflush = entity.Version;
				
				Assert.That(versionAfterflush, Is.EqualTo(versionBeforeFlush), "The entity version must not change when update is vetoed");
			}
		}
		
		// A listener always returning true
		public partial class TestEventListener : IPreUpdateEventListener
		{
			public bool Executed { get; set; }
			public bool FoundAny { get; set; }

			public bool OnPreUpdate(PreUpdateEvent @event)
			{
				return true;
			}
		}

		public partial class Entity
		{
			public virtual int Id { get; set; }
			public virtual string Name { get; set; }
			public virtual int Version { get; set; }
		}
	}
}
