using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH925
{
    public class Hobby
    {
        private Guid? _id;
        private string _name = null;
        private IList<Person> _people = new List<Person>();

        public Hobby()
        {
        }

        public Hobby(string name)
            : this()
        {
            _name = name;
        }

        public virtual Guid? Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public virtual string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public virtual IList<Person> People
        {
            get { return _people; }
            set { _people = value; }
        }
    }
}
