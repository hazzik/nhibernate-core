using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1020
{
	public class Parent
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual ISet<Child> Children { get; set; } = new HashSet<Child>();
	}

	public class ChildId
	{
		public virtual Parent Parent { get; set; }
		public virtual int Sequence { get; set; }

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			if (!(obj is ChildId other))
				return false;

			return Equals(Parent?.Id, other.Parent?.Id) && Sequence == other.Sequence;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Parent?.Id.GetHashCode() ?? 0) * 397) ^ Sequence;
			}
		}
	}

	public class Child
	{
		public virtual ChildId Id { get; set; }
		public virtual string Name { get; set; }
	}
}
