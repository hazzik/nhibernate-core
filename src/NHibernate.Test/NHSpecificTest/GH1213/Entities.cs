using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1213
{
	public class Parent
	{
		public virtual int Id { get; set; }
		public virtual int Weight { get; set; }

		// Read-only, formula-backed property: emitting it in an ORDER BY produces
		// a parenthesized expression, e.g. "order by (Weight) asc", which is what
		// a projection-based order by looks like.
		public virtual int SortKey { get; protected set; }

		public virtual IList<Child> Children { get; set; } = new List<Child>();
	}

	public class Child
	{
		public virtual int Id { get; set; }
		public virtual Parent Parent { get; set; }
	}
}
