using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1162
{
	public class Parent
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<Child> Children { get; set; } = new List<Child>();
	}

	public class Child
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
