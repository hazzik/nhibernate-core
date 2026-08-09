using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1350
{
	public class Container
	{
		public virtual int Id { get; set; }
		public virtual IDictionary<Tag, Element> Items { get; set; } = new Dictionary<Tag, Element>();
	}

	// The map value is a one-to-many entity. Its own "Tag" many-to-one association points to the very same
	// column that the map (see FixtureByCode.GetMappings) reuses as its key-many-to-many index, forming the
	// "ternary" relationship described in the issue: Container -> Element (one-to-many) -> Tag (many-to-one),
	// with Tag also acting as the map key.
	public class Element
	{
		public virtual int Id { get; set; }
		public virtual Tag Tag { get; set; }
		public virtual string Value { get; set; }
	}

	// A key entity whose equality is based on its business state rather than its identifier, exactly the case
	// described in the issue: the hash code differs before and after the entity is fully hydrated.
	public class Tag
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }

		public override bool Equals(object obj)
		{
			return obj is Tag other && Name == other.Name;
		}

		public override int GetHashCode()
		{
			return Name?.GetHashCode() ?? 0;
		}
	}
}
