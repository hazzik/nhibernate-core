using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using NHibernate.Criterion;
using NHibernate.Dialect;
using NHibernate.Linq;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2379
{
    [TestFixture]
    public class JoinTest : BugTestCase
    {
        
        private MemoryAppender memoryAppender;
        

        protected override void OnTearDown()
        {
            if (memoryAppender!=null) {
                Hierarchy repository = (Hierarchy) LogManager.GetRepository();
                repository.Root.RemoveAppender(memoryAppender);
                memoryAppender = null;
            }
            base.OnTearDown();
        }
       
        [Test]
        public void JoinWithFilterTest() {
            using (ISession session = OpenSession()) {
                DateTime filterDate = DateTime.Today;
                var list = session.Query<Organisation>().Join(session.Query<ResponsibleLegalPerson>().Where(p=> p.StartDate<=filterDate), organisation => organisation,
                                                          r => r.Organisation,
                                                          (organisation, r) => new {Organisation=organisation, LegalPerson=r}).ToList();
                
            }
            
        }

        [Test]
        public void LeftJoinTest() {
            using (ISession session = OpenSession()) {
                DateTime filterDate = DateTime.Today;
                var list = session.Query<Organisation>().Join(session.Query<ResponsibleLegalPerson>().DefaultIfEmpty(), organisation => organisation,
                                                          r => r.Organisation,
                                                          (organisation, r) => new {Organisation=organisation, LegalPerson=r}).ToList();
                
            }
            
        }

        [Test]
        public void JoinTest2() {
            using (ISession session = OpenSession()) {
                DateTime filterDate = DateTime.Today;
                
                var list = 
                    from o in session.Query<Organisation>()
                    join p in session.Query<ResponsibleLegalPerson>() on o equals p.Organisation into something
                    from p in something.DefaultIfEmpty()
                    select new {Organisation = o, Person = p};
                list.ToList();
            }
            
        }

        /// <summary>
        /// this one works
        /// </summary>
        [Test]
        public void JoinTest3() {
            using (ISession session = OpenSession()) {
                DateTime filterDate = DateTime.Today;
                
                var list = 
                    from o in session.Query<Organisation>()
                    join p in session.Query<ResponsibleLegalPerson>() on o equals p.Organisation 
                    select new {Organisation = o, Person = p};
                
                list.ToList();
            }
            
        }

        [Test]
        public void JoinTest4() {
            using (ISession session = OpenSession()) {
                DateTime filterDate = DateTime.Today;
                
                var list = 
                    from o in session.Query<Organisation>()
                    join p in (from x in session.Query<ResponsibleLegalPerson>() where  x.StartDate<filterDate select  x) on o equals p.Organisation 
                    select new {Organisation = o, Person = p};
                
                list.ToList();
            }
            
        }

    }
}
