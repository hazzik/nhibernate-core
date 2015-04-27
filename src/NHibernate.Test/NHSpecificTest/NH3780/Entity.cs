using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHibernate.Test.NHSpecificTest.NH3780
{
    public class Entity
    {
        public virtual int Id { get; set; }
        public virtual string Name { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
}
