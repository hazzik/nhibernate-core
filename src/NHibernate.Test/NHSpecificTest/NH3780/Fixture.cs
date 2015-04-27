using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NHibernate.Cfg;

namespace NHibernate.Test.NHSpecificTest.NH3780
{
    [TestFixture]
    public class Fixture : BugTestCase
    {
        [Test]
        public void AreQueryCached()
        {
            var oddIds = new List<int>();
            var eventIds = new List<int>();
            
            using (var session = OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    int i = 1000;
                    while (i-- > 0) {
                        if (i % 2 == 0) oddIds.Add(i);
                        if (i % 2 != 0) eventIds.Add(i);
                        session.Save(new NH3780.Entity());
                    };
                    session.Flush();
                    transaction.Commit();
                }

                var oddQuery = session.Query<NH3780.Entity>().Where(entity => entity.Id.In(oddIds));
                var evenQuery = session.Query<NH3780.Entity>().Where(entity => entity.Id.In(eventIds));
                var oddResult = oddQuery.ToArray();
                var evenResult = evenQuery.ToArray();
                Assert.IsTrue(evenResult.All(entity => entity.Id % 2 != 0));

                using (var transaction = session.BeginTransaction())
                {
                    session.Delete("from Entity");
                    session.Flush();
                    transaction.Commit();
                }
            }
        }
        protected override void Configure(Configuration configuration)
        {
            configuration.LinqToHqlGeneratorsRegistry<CustomLinqGeneratorsRegistry>();
            base.Configure(configuration);
        }
    }
}
