﻿using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using NHibernate.Util;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3487
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		private Key _key1;
		private Key _key2;

		protected override void OnSetUp()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					_key1 = new Key {Id = 1};
					var entity1 = new Entity {Id = _key1, Name = "Bob1"};
					session.Save(entity1);

					_key2 = new Key {Id = 2};
					var entity2 = new Entity {Id = _key2, Name = "Bob2"};
					session.Save(entity2);

					session.Flush();
					transaction.Commit();
				}
			}
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					session.Delete("from System.Object");

					session.Flush();
					transaction.Commit();
				}
			}
		}

#if !NET5_0_OR_GREATER
		[Test]
		public void CanDeserializeSessionWithEntityHashCollision()
		{
			var formatter = new BinaryFormatter
			{
#if !NETFX
				SurrogateSelector = new SerializationHelper.SurrogateSelector()	
#endif
			};
			byte[] serializedSessionArray;

			using (ISession session = OpenSession())
			{
				using (session.BeginTransaction())
				{
					session.Get<Entity>(_key1);
					session.Get<Entity>(_key2);
				}

				session.Disconnect();
				using (var serializationStream = new MemoryStream())
				{
					formatter.Serialize(serializationStream, session);
					serializationStream.Close();
					serializedSessionArray = serializationStream.ToArray();
				}
			}

			using (var serializationStream = new MemoryStream(serializedSessionArray))
			{
				formatter.Deserialize(serializationStream);
			}
		}
#endif
	}
}
