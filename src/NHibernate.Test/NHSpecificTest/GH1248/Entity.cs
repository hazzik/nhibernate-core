using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace NHibernate.Test.NHSpecificTest.GH1248
{
	class Basket
	{
		private readonly ICollection<Apple> _apples = new List<Apple>();
		private readonly ICollection<Orange> _oranges = new List<Orange>();

		public virtual int Id { get; set; }
		public virtual ICollection<Apple> Apples => _apples;
		public virtual ICollection<Orange> Oranges => _oranges;
	}

	abstract class Fruit
	{
		public virtual int Id { get; set; }
		public virtual Basket Basket { get; set; }
	}

	class Apple : Fruit
	{
	}

	class Orange : Fruit
	{
	}
}
