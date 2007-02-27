using System;
using NHibernate.Criterion;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH925
{
    [TestFixture]
    public class Fixture : BugTestCase
    {
        protected override void OnSetUp()
        {
            using (var session = OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                var dave = new Person("dave");
                session.Save(dave);
                var bob = new Person("bob");
                session.Save(bob);

                var running = new Hobby("running");
                session.Save(running);
                var swimming = new Hobby("swimming");
                session.Save(swimming);

                // custom <sql-insert> is correctly called
                dave.Hobbies.Add(running);
                dave.Hobbies.Add(swimming);
                bob.Hobbies.Add(swimming);

                session.Flush();

                // this should hopefully call the <sql-delete> custom query
                // but it instead generates dynamic sql
                dave.Hobbies.Remove(running);

                transaction.Commit();
            }
        }

        protected override void OnTearDown()
        {
            using (var session = OpenSession())
            using (var transaction = session.BeginTransaction())
            {
                session.Delete("from System.Object");
                transaction.Commit();
            }
        }

        [Test]
        public void Test()
        {
            using (var session = OpenSession())
            using (session.BeginTransaction())
            {
                var dave = session.CreateCriteria(typeof (Person)).Add(Restrictions.Eq("Name", "dave")).UniqueResult<Person>();
                // invoke lazy load of hobbies (this throws an exception);
                var count = dave.Hobbies.Count;
            }
        }
    }
}