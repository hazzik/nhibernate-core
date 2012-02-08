using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2379
{
    public class Parent
    {
        public Parent()
        {
            Children = new List<Child>();
        }

        public virtual int Id { get; set; }

        public virtual IList<Child> Children { get; set; }
    }
}