using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1297
{
	class Animal
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class Cat : Animal
	{
		public virtual int NumberOfLegs { get; set; }
	}

	class Zoo
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<Animal> Animals { get; set; } = new List<Animal>();
	}
}
