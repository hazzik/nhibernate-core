using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH925
{
    public class Person
    {
        private Guid? _id;
        private string _name = null;
        private IList<Hobby> _hobbies = new List<Hobby>();

        public Person()
        {
        }

        public Person(string name) : this()
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

        public virtual IList<Hobby> Hobbies
        {
            get { return _hobbies; }
            set { _hobbies = value; }
        }
    }
}
