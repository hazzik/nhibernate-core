using System;

namespace NHibernate.Test.NHSpecificTest.GH1040
{
	class Item
	{
		public virtual ItemId Id { get; set; }
		public virtual string Name { get; set; }

		// Many-to-one to an entity with the same composite id type. Its first key column
		// (PhaseId) is the same column used by this entity's own composite id, while the
		// second (CopiedNum) is a genuinely distinct column.
		public virtual Item WasCopiedFrom { get; set; }
	}

	class ItemId : IEquatable<ItemId>
	{
		public virtual int PhaseId { get; set; }
		public virtual int Num { get; set; }

		public virtual bool Equals(ItemId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return PhaseId == other.PhaseId && Num == other.Num;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ItemId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (PhaseId * 397) ^ Num;
			}
		}
	}
}
