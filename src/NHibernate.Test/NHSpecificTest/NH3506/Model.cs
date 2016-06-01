using System;

namespace NHibernate.Test.NHSpecificTest.NH3506
{
	public class BaseClass
	{
		public virtual Guid Id { get; set; }
	}

    public class Employee : BaseClass
    {
        public virtual Department Department { get; set; }
    }

    public class Department : BaseClass
    {
    }
}
