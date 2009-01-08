using System.Collections;
using System.Reflection;
using NHibernate.Cfg;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH945
{
    [TestFixture]
    public class ComponentSubclassTests
    {
        [Test]
        public void TestSubclasses()
        {
            Configuration cfg = new Configuration();
            cfg.AddResource("NHibernate.Test.NHSpecificTest.NH945.Mappings.hbm.xml", Assembly.Load("NHibernate.Test"));
            cfg.BuildSessionFactory();
        }
    }
}