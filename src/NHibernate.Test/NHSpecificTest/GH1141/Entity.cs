using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace NHibernate.Test.NHSpecificTest.GH1141
{
	public class Purchase
	{
		public virtual int Id { get; set; }
		public virtual string Number { get; set; }
		public virtual IList<Item> Items { get; set; } = new List<Item>();
	}

	public class Item
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
