using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1294
{
	public class RootEntity
	{
		public virtual int Id { get; set; }
		public virtual BaseJoin BaseJoin { get; set; }
	}

	public class BaseJoin
	{
		public virtual int Id { get; set; }
	}

	// Two sibling subclasses expose a collection property with the same name.
	public class SpecificJoin : BaseJoin
	{
		public virtual IList<Target> Problematic { get; set; } = new List<Target>();
	}

	public class SiblingJoin : BaseJoin
	{
		public virtual IList<Target> Problematic { get; set; } = new List<Target>();
	}

	public class Target
	{
		public virtual int Id { get; set; }
	}
}
