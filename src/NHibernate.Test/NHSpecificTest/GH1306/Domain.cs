using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1306
{
	public class Parent
	{
		public virtual long Id { get; set; }
		public virtual ISet<Child> Children { get; set; } = new HashSet<Child>();
	}

	public class Child
	{
		public virtual long Id { get; set; }
		public virtual long ParentId { get; set; }
		public virtual int Status { get; set; }
	}
}
