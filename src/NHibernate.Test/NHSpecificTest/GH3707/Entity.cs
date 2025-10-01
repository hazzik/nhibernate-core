using System;

namespace NHibernate.Test.NHSpecificTest.GH3707
{
	partial class Entity
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		
		public virtual DateTime DateTime1 { get; set; }
		public virtual DateTime DateTime2 { get; set; }
	}
}
