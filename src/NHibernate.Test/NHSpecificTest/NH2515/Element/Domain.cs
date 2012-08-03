using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2515.Element
{
	public class Parent
	{
		public Parent()
		{
			Children = new List<string>();
		}

		public virtual Guid Id { get; set; }

		public virtual ICollection<string> Children { get; set; }
	}
}
