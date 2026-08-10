using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1061
{
	class Parent
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<ChildValue> Children { get; set; } = new List<ChildValue>();
	}

	// Mapped as a composite-element (a value type, not an entity in its own right).
	class ChildValue
	{
		public virtual string Label { get; set; }

		// Mapped with insert="false" update="false": NHibernate must never write this column.
		public virtual string ReadOnlyValue { get; set; }
	}
}
