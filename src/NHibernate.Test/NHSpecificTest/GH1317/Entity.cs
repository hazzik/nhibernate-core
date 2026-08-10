using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1317
{
	class Root
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual ISet<ChildA> As { get; set; } = new HashSet<ChildA>();
		public virtual ISet<ChildB> Bs { get; set; } = new HashSet<ChildB>();
	}

	class ChildA
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class ChildB
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
