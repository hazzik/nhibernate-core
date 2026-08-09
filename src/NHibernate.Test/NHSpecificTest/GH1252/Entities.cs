using System.Collections.Generic;
using System.Linq;

namespace NHibernate.Test.NHSpecificTest.GH1252
{
	// A Toy is a simple entity with no references to other entities.
	public class Toy
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	// Every time a Child is instantiated (including by NHibernate, when hydrating it from the
	// database), a brand new Toy is created and assigned to it. DynamicToy is mapped as
	// access="readonly": NHibernate reads it via the getter but never assigns it back, so the
	// freshly constructed, still-transient Toy always wins over whatever was loaded from the row.
	public class Child
	{
		private readonly Toy _dynamicToy = new Toy { Name = "Generated toy" };

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Parent Parent { get; set; }
		public virtual Toy DynamicToy => _dynamicToy;
	}

	public class Parent
	{
		private readonly IList<Child> _children = new List<Child>();

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<Child> Children => _children;
		public virtual ParentSummary Summary { get; set; }
	}

	// FirstChild is computed from Parent.Children, and mapped access="readonly" + cascade="all"
	// so that, at flush time, cascading into ParentSummary forces Parent.Children (still lazy at
	// that point) to be loaded from the database.
	public class ParentSummary
	{
		public virtual int Id { get; set; }
		public virtual Parent Parent { get; set; }
		public virtual Child FirstChild => Parent?.Children?.FirstOrDefault();
	}
}
