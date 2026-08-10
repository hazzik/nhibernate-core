using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH2028
{
	class Parent
	{
		public virtual string Id { get; set; }
		public virtual string ParentName { get; set; }
		public virtual ISet<Child> Childs { get; set; } = new HashSet<Child>();
	}

	class Child
	{
		public virtual string Id { get; set; }
		public virtual string ChildName { get; set; }
	}
}
