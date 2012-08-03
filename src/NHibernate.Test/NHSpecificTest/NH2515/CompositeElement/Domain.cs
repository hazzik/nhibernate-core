using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2515.CompositeElement
{
	public class Child
	{
		public virtual string Name { get; set; }
	}

	public class Parent
	{
		public Parent()
		{
			Children = new List<Child>();
		}

		public virtual Guid Id { get; set; }

		public virtual ICollection<Child> Children { get; set; }
	}
}
