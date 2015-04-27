using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Linq;
using NHibernate.Cfg;
using NHibernate.Engine;
using System.Reflection;
using NHibernate.Engine.Query;
using System.Linq.Expressions;
using NHibernate.Util;

namespace NHibernate.Test.NHSpecificTest.NH3780
{
    //https://nhibernate.jira.com/browse/NH-3780
    [TestFixture]
    public class Fixture : BugTestCase
    {
        [Test]
        public void AreQueryPlanWellCached()
        {
            var oddIds = new List<int>();
            var evenIds = new List<int>();
            
            using (var session = OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    int i = 1000;
                    while (i-- > 0) {
                        if (i % 2 == 0) oddIds.Add(i);
                        if (i % 2 != 0) evenIds.Add(i);
                        session.Save(new NH3780.Entity());
                    };
                    session.Flush();
                    transaction.Commit();
                }

                var oddQuery = session.Query<NH3780.Entity>().Where(entity => entity.Id.In(oddIds));
                var evenQuery = session.Query<NH3780.Entity>().Where(entity => entity.Id.In(evenIds));
                var oneMoreOddQuery = session.Query<NH3780.Entity>().Where(entity => entity.Id.In(oddIds));

                var sessionImplementor = session.GetSessionImplementation();
                var sessionFactoryImplementor = sessionImplementor.Factory;
                var queryPlanCache = sessionFactoryImplementor.QueryPlanCache;

                var oddQueryExpression = new NhLinqExpression(oddQuery.Expression, sessionFactoryImplementor);
                var evenQueryExpression = new NhLinqExpression(evenQuery.Expression, sessionFactoryImplementor);
                var oneMoreOddQueryExpression = new NhLinqExpression(oneMoreOddQuery.Expression, sessionFactoryImplementor);

                var oddPlan = queryPlanCache.GetHQLQueryPlan(oddQueryExpression, false, new CollectionHelper.EmptyMapClass<string, IFilter>());
                var evenPlan = queryPlanCache.GetHQLQueryPlan(evenQueryExpression, false, new CollectionHelper.EmptyMapClass<string, IFilter>());
                var oneMoreOddPlan = queryPlanCache.GetHQLQueryPlan(oneMoreOddQueryExpression, false, new CollectionHelper.EmptyMapClass<string, IFilter>());

                Assert.AreNotEqual(oddPlan.QueryExpression.Key, evenPlan.QueryExpression.Key);
                Assert.AreEqual(oddPlan.QueryExpression.Key, oneMoreOddPlan.QueryExpression.Key);

                var oddResult = oddQuery.ToArray();
                var evenResult = evenQuery.ToArray();
                var oneMoreOddResult = oneMoreOddQuery.ToArray();

                Assert.IsTrue(evenResult.All(entity => entity.Id % 2 != 0));
                Assert.IsTrue(oddResult.All(id => oneMoreOddResult.Contains(id)) && oneMoreOddResult.All(id => oddResult.Contains(id)));

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
