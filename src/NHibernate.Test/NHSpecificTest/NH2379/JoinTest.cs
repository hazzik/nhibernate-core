using System;
using System.Linq;
using NHibernate.Linq;
using NUnit.Framework;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace NHibernate.Test.NHSpecificTest.NH2379
{
    [TestFixture]
    public class JoinTest : BugTestCase
    {
        private MemoryAppender memoryAppender;


        protected override void OnTearDown()
        {
            if (memoryAppender != null)
            {
                var repository = (Hierarchy) LogManager.GetRepository();
                repository.Root.RemoveAppender(memoryAppender);
                memoryAppender = null;
            }
            base.OnTearDown();
        }

        [Test]
        public void JoinTest1()
        {
            using (var session = OpenSession())
            {
                var list =
                    from o in session.Query<Organisation>()
                    join p in session.Query<ResponsibleLegalPerson>().DefaultIfEmpty() on o equals p.Organisation into something
                    from p in something
                    select new {Organisation = o, Person = p};

                list.ToList();
            }
        }

        [Test]
        public void JoinTest2()
        {
            using (var session = OpenSession())
            {
                var list =
                    from o in session.Query<Organisation>()
                    join p in session.Query<ResponsibleLegalPerson>() on o equals p.Organisation into something
                    from p in something.DefaultIfEmpty()
                    select new {Organisation = o, Person = p};

                list.ToList();
            }
        }

        /// <summary>
        ///   this one works
        /// </summary>
        [Test]
        public void JoinTest3()
        {
            using (var session = OpenSession())
            {
                var list =
                    from o in session.Query<Organisation>()
                    join p in session.Query<ResponsibleLegalPerson>() on o equals p.Organisation
                    select new {Organisation = o, Person = p};

                list.ToList();
            }
        }

        [Test]
        public void JoinTest4()
        {
            using (var session = OpenSession())
            {
                var filterDate = DateTime.Today;

                var persons = from x in session.Query<ResponsibleLegalPerson>()
                              //where x.StartDate < filterDate
                              select x;

                var list = from o in session.Query<Organisation>()
                           join p in persons on o equals p.Organisation
                           select new {Organisation = o, Person = p};

                list.ToList();
            }
        }
    }
}