using System;

namespace NHibernate.Test.NHSpecificTest.GH1265
{
	class PersonChange : IEquatable<PersonChange>
	{
		public virtual int? ChangeId { get; set; }
		public virtual int? ChangeSystemId { get; set; }
		public virtual Person Person { get; set; }

		public virtual bool IsExportable { get; set; }

		public virtual bool Equals(PersonChange other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return ChangeId == other.ChangeId &&
			       ChangeSystemId == other.ChangeSystemId &&
			       Person?.Id == other.Person?.Id;
		}

		public  override bool Equals(object obj)
		{
			return Equals(obj as PersonChange);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = ChangeId.GetHashCode();
				hashCode = (hashCode * 397) ^ ChangeSystemId.GetHashCode();
				hashCode = (hashCode * 397) ^ (Person != null ? Person.GetHashCode() : 0);
				return hashCode;
			}
		}
	}

	class Person
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
