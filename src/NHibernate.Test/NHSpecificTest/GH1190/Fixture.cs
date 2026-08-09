using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1190
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Animal>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				rc.Property(x => x.Description);
			});

			mapper.JoinedSubclass<Human>(rc =>
			{
				rc.Property(x => x.NickName);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new Human { Description = "human", NickName = "Bob" });

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				// HQL Delete of entities with joins requires temp tables, which are not
				// supported by all dialects: use in memory-delete instead.
				session.Delete("from System.Object");

				transaction.Commit();
			}
		}

		[Test]
		public void MultiTableUpdateWithConcatFunction()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var count = session
					.CreateQuery("update Human h set h.Description = concat(h.Description, :p)")
					.SetParameter("p", " a")
					.ExecuteUpdate();

				Assert.That(count, Is.EqualTo(1));

				transaction.Commit();
			}

			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var human = session.Query<Human>().Single();
				Assert.That(human.Description, Is.EqualTo("human a"));
			}
		}
	}
}
