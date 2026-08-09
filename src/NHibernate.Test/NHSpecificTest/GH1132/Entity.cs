using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace NHibernate.Test.NHSpecificTest.GH1132
{
	class Foo
	{
		private readonly ICollection<Bar> _bars = new List<Bar>();
		public virtual int Id { get; set; }
		public virtual ICollection<Bar> Bars => _bars;
	}

	class Bar
	{
		public virtual int Id { get; set; }
	}

	class Baz
	{
		public virtual int Id { get; set; }
	}
}
