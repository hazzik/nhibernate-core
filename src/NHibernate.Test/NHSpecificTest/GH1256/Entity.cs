using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1256
{
	public abstract class AbstractParent
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class Parent : AbstractParent
	{
		public Parent()
		{
			Children = new List<Child>();
		}

		public virtual IList<Child> Children { get; set; }
	}

	public class Child
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Parent Parent { get; set; }
	}
}
