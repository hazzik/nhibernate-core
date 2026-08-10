using System.Collections.Generic;
using System.Linq;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable ClassNeverInstantiated.Global

namespace NHibernate.Test.NHSpecificTest.GH1039
{
	class Entity
	{
		public virtual int Id { get; set; }

		public virtual ICollection<Child> Children { get; set; } = new List<Child>();
	}

	class Child
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class EntityDto
	{
		public int Id { get; set; }
		public IEnumerable<string> ChildNames { get; set; } = Enumerable.Empty<string>();
	}
}
