using System.Collections;
using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1839
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override bool AppliesTo(Engine.ISessionFactoryImplementor factory)
		{
			return factory.ConnectionProvider.Driver.SupportsMultipleQueries;
		}

		protected override void Configure(Configuration configuration)
		{
			configuration.SetProperty(Environment.GenerateStatistics, "true");
		}

		protected override void OnSetUp()
		{
			base.OnSetUp();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.Save(new Entity {Id = 1});
				t.Commit();
			}
		}

		[Test]
		public void MultiCriteriaShouldTriggerSessionFlushIfExecutedAfterDelete()
		{
			this.sessions.Statistics.Clear();
			using (var s = OpenSession())
			{
				s.FlushMode = FlushMode.Auto;

				using (var t = s.BeginTransaction())
				{
					var entity = s.Load<Entity>(1);
					s.Delete(entity);

					var results = s.CreateMultiCriteria()
						.Add(s.CreateCriteria<Entity>())
						.List();

					Assert.AreEqual(1, this.sessions.Statistics.EntityDeleteCount);
					Assert.AreEqual(1, this.sessions.Statistics.FlushCount);
					Assert.AreEqual(0, ((IList)results[0]).Count);
				}
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();

			using (var s = OpenSession())
			using (var t = s.BeginTransaction())
			{
				s.CreateQuery("delete from System.Object").ExecuteUpdate();
				t.Commit();
			}
		}
	}
}