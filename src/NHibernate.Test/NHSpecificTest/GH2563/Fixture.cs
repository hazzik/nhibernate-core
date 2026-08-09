using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2563
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.Name, m => m.Lazy(true));
					rc.ManyToOne(x => x.Predecessor, m => m.ForeignKey("none"));
					rc.ManyToOne(x => x.Successor, m => m.ForeignKey("none"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new Entity {Id = 1, Name = "EntityA"});
				session.Save(new Entity {Id = 2, Name = "EntityB"});
				transaction.Commit();
			}

			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entityA = session.Get<Entity>(1);
				var entityB = session.Get<Entity>(2);

				// Two mutual many-to-one self references, so that entityA and entityB
				// end up depending on each other through two different IObjectReference
				// instances (FieldInterceptorObjectReference), one per lazy-property entity.
				entityA.Successor = entityB;
				entityB.Predecessor = entityA;

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
		public void CanDeserializeSessionContainingEntitiesWithLazyPropertiesAndCircularSelfReferences()
		{
			using (var session = OpenSession())
			{
				// Force both entities to be loaded as fully initialized instances (not proxies)
				// referencing each other, so serialization goes through FieldInterceptorObjectReference
				// on both sides of the circular reference.
				var list = session.QueryOver<Entity>()
				                   .Fetch(SelectMode.Fetch, e => e.Predecessor)
				                   .Fetch(SelectMode.Fetch, e => e.Successor)
				                   .List();

				Assert.That(list, Has.Count.EqualTo(2));

				object deserializedSession = null;
				Assert.DoesNotThrow(
					() => deserializedSession = SpoofSerialization(session),
					"Deserializing the session should not throw a SerializationException due to " +
					"mutually dependent IObjectReference instances.");

				Assert.That(deserializedSession, Is.Not.Null);
			}
		}

		private static T SpoofSerialization<T>(T obj)
		{
			var formatter = new BinaryFormatter
			{
#if !NETFX
				SurrogateSelector = new NHibernate.Util.SerializationHelper.SurrogateSelector()
#endif
			};
			var stream = new MemoryStream();
			formatter.Serialize(stream, obj);

			stream.Position = 0;

			return (T) formatter.Deserialize(stream);
		}
	}
}
