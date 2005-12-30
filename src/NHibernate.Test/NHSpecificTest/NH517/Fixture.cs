using System;
using System.Collections;

using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH517
{
    [TestFixture]
    public class Fixture : BugTestCase
    {
        /* Two tests are supplied. The one giving the extra SELECTS is: DoTestIncome
	     * I do not know how to assert the amount of sql sent to the db, therefor it will not fail.
	     * But if you look in your output, you will notice the unnecessary call: SELECT nh517paren0_.ParentID as ParentID0_ FROM tstNH517Parent nh517paren0_ WHERE nh517paren0_.ParentID=:p0
	     * That one is not there in the DoTestChildren tst.
	     * */
        [Test]
        public void DoTestChildren()
        {
            NH517Parent p;


            using (ISession s = OpenSession())
            {
                // insert one parent and 2 children
                p = new NH517Parent();
                p.Children = new ArrayList(2);
                p.Identity = new NH517ParentKey();
                p.Identity.ParentID = 1;
                s.Save(p);

                NH517Child c1 = new NH517Child();
                c1.Identity = new NH517ChildKey();
                c1.Identity.ParentID = 1;
                c1.Identity.Counter = 1;
                c1.Parent = p;
                c1.Name = "Ruurd";
                p.Children.Add(c1);


                NH517Child c2 = new NH517Child();
                c2.Identity = new NH517ChildKey();
                c2.Identity.ParentID = 1;
                c2.Identity.Counter = 2;
                c2.Parent = p;
                c2.Name = "Laura";
                p.Children.Add(c2);


                s.Save(c2);
                s.Save(c1);
                s.Flush();

            }

            Console.WriteLine("ready, following statements are graph loading");


            // select the graph
            using (ISession s1 = OpenSession())
            {
                NH517ParentKey p2ID = new NH517ParentKey();
                p2ID.ParentID = 1;
                IList pLst = s1.CreateQuery("from NH517Parent p inner join fetch p.Children").List();

            }

            Console.WriteLine("ready, following statements are deletion of p");

            using (ISession s2 = OpenSession())
            {
                s2.Delete(p);
                s2.Flush();
            }
            Console.WriteLine("ready, following statements are teardown");
        }


        [Test]
        public void DoTestIncome()
        {
            NH517Parent p;

            using (ISession s = OpenSession())
            {
                // insert one parent and 2 children
                p = new NH517Parent();
                p.Income = new ArrayList(2);
                p.Identity = new NH517ParentKey();
                p.Identity.ParentID = 1;
                s.Save(p);


                Income i1 = new Income();
                i1.Identity = new IncomeKey();
                i1.Identity.Client = p;
                i1.Identity.IncomeCategory = 1;
                p.Income.Add(i1);

                Income i2 = new Income();
                i2.Identity = new IncomeKey();
                i2.Identity.Client = p;
                i2.Identity.IncomeCategory = 4;
                p.Income.Add(i2);



                s.Save(i2);
                s.Save(i1);
                s.Flush();

            }

            Console.WriteLine("ready, following statements are graph loading");

            // select the graph
            IList pLst;
            using (ISession s1 = OpenSession())
            {
                pLst = s1.CreateQuery("from NH517Parent p inner join fetch p.Income").List();
            }

            Console.WriteLine("ready, following statements are deletion of p");

            using (ISession s2 = OpenSession())
            {
                s2.Delete(p);
                s2.Flush();
            }
            Console.WriteLine("ready, following statements are teardown");
        }
    }
}
