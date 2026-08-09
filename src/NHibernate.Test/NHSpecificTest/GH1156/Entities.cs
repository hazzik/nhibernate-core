using System;

namespace NHibernate.Test.NHSpecificTest.GH1156
{
	public class ExternalFieldId : IEquatable<ExternalFieldId>
	{
		public virtual int CommonId { get; set; }
		public virtual int Id { get; set; }

		public bool Equals(ExternalFieldId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return CommonId == other.CommonId && Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as ExternalFieldId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (CommonId * 397) ^ Id;
			}
		}
	}

	public class ExternalField
	{
		public virtual ExternalFieldId Id { get; set; }
		public virtual string SomeProp { get; set; }
	}

	public class FieldId : IEquatable<FieldId>
	{
		public virtual int CommonId { get; set; }
		public virtual int Id { get; set; }

		public bool Equals(FieldId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return CommonId == other.CommonId && Id == other.Id;
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as FieldId);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (CommonId * 397) ^ Id;
			}
		}
	}

	public class Field
	{
		public virtual FieldId Id { get; set; }

		// Backs the second (non-shared) column of the many-to-one below. The first column,
		// CommonId, is shared with this entity's own composite id, as in the original report.
		public virtual int? ExternalFieldRefId { get; set; }

		public virtual ExternalField ExternalField { get; set; }
	}
}
