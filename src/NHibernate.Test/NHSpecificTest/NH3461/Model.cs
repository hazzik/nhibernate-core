using System;

namespace NHibernate.Test.NHSpecificTest.NH3461
{
	public class Parent
	{
		private Guid id = Guid.Empty;

		public Parent(string name)
		{
			this.Name = name;
		}

		private Parent()
		{
		}

		public string Name { get; private set; }
	}

	public class Child
	{
		private Guid id = Guid.Empty;

		public Child(string name, Parent parent)
		{
			this.Name = name;
			this.Parent = parent;
		}

		private Child()
		{
		}

		public string Name { get; private set; }

		public Parent Parent { get; private set; }
	}
}
