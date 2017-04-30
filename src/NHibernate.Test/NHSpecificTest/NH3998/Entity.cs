using System;

namespace NHibernate.Test.NHSpecificTest.NH3998
{
	class Entity
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string NaMe { get; set; }
	}
}