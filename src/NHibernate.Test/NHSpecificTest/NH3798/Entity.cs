using System;

namespace NHibernate.Test.NHSpecificTest.NH3798
{
	public class Entity
	{
		public virtual Guid Id { get; set; }

		public virtual Entity Parent { get; set; }
	}

	public class SubEntity1 : Entity
	{
	}

	public class SubEntity2 : Entity
	{
	}
}
