using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1787
{
	class Parent
	{
		private readonly ICollection<Child> _children = new List<Child>();

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual ICollection<Child> Children => _children;
	}

	class Child
	{
		public virtual int Id { get; set; }
		public virtual Parent Parent { get; set; }
	}
}
