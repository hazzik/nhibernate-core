using System;

namespace NHibernate.Test.NHSpecificTest.GH2563
{
	[Serializable]
	public class Entity
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Entity Predecessor { get; set; }
		public virtual Entity Successor { get; set; }
	}
}
