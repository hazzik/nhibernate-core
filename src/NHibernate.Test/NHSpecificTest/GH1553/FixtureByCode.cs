using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1553
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(
				m =>
				{
					m.Id(x => x.Id, i => i.Generator(Generators.Assigned));
					m.Property(x => x.Name);
				});
			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new Entity {Id = 1, Name = "entity1"});
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
		public void StatelessSessionSurvivesSerializationRoundTrip()
		{
			using (var statelessSession = Sfi.OpenStatelessSession())
			{
				var deserialized = SpoofSerialization(statelessSession);

				var entity = deserialized.Get<Entity>(1);

				Assert.That(entity, Is.Not.Null);
				Assert.That(entity.Name, Is.EqualTo("entity1"));
			}
		}

		private static T SpoofSerialization<T>(T obj)
		{
			var formatter = new BinaryFormatter
			{
#if !NETFX
				SurrogateSelector = new Util.SerializationHelper.SurrogateSelector()
#endif
			};
			var stream = new MemoryStream();
			formatter.Serialize(stream, obj);

			stream.Position = 0;

			return (T) formatter.Deserialize(stream);
		}
	}
}
