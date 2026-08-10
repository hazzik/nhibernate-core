using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1057
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(i => i.Id, m => m.Generator(Generators.GuidComb));
				rc.Component(p => p.FirstComponent,
					m =>
					{
						// table name omitted, expecting a reasonable default
						m.Set(c => c.ComponentCollection,
							c => { },
							c => c.Element());
						// column name omitted, expecting a reasonable default
						m.Property(p => p.ComponentProperty);
					});
				rc.Component(p => p.SecondComponent,
					m =>
					{
						// table name omitted, expecting a reasonable default
						m.Set(c => c.ComponentCollection,
							c => { },
							c => c.Element());
						// column name omitted, expecting a reasonable default
						m.Property(p => p.ComponentProperty);
					});
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
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
		public void ComponentsOfSameTypeWithoutExplicitColumnsDoNotOverlap()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var entity = new Entity
				{
					FirstComponent = new Component
					{
						ComponentProperty = "First",
						ComponentCollection = new List<string> { "FirstOne", "FirstTwo" }
					},
					SecondComponent = new Component
					{
						ComponentProperty = "Second",
						ComponentCollection = new List<string> { "SecondOne", "SecondTwo", "SecondThree" }
					}
				};

				session.Save(entity);
				transaction.Commit();
			}

			using (var session = OpenSession())
			{
				var entity = session.Query<Entity>().Single();

				Assert.That(entity.FirstComponent.ComponentProperty, Is.EqualTo("First"));
				Assert.That(entity.SecondComponent.ComponentProperty, Is.EqualTo("Second"));
				Assert.That(entity.FirstComponent.ComponentCollection, Is.EquivalentTo(new[] { "FirstOne", "FirstTwo" }));
				Assert.That(entity.SecondComponent.ComponentCollection, Is.EquivalentTo(new[] { "SecondOne", "SecondTwo", "SecondThree" }));
			}
		}
	}
}
