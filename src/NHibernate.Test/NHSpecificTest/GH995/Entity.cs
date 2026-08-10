using System;

namespace NHibernate.Test.NHSpecificTest.GH995
{
	public class Parent
	{
		public virtual Guid Id { get; set; }
	}

	public class Parent1 : Parent
	{
	}

	public class Parent2 : Parent
	{
	}

	public class Child
	{
		public virtual Guid Id { get; set; }
		public virtual Guid? ParentId { get; set; }

		// Query-only association: the FK is already exposed through ParentId above,
		// so this many-to-one is never read from nor written to the database row.
		public virtual Parent Parent { get; set; }
	}
}
