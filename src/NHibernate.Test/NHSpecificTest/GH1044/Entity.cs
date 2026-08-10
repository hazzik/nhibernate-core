namespace NHibernate.Test.NHSpecificTest.GH1044
{
	// The properties are deliberately declared in the opposite order to how they are
	// mapped inside ComposedId below, so a reproduction can tell whether the generated
	// column order follows the mapping call order (correct) or the class declaration
	// order (the reported bug).
	public class Parent
	{
		public virtual string StrKey { get; set; }
		public virtual int IntKey { get; set; }

		public override bool Equals(object obj)
		{
			return obj is Parent other && StrKey == other.StrKey && IntKey == other.IntKey;
		}

		public override int GetHashCode()
		{
			return (StrKey, IntKey).GetHashCode();
		}
	}
}
