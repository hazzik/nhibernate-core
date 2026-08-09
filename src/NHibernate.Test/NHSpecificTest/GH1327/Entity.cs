namespace NHibernate.Test.NHSpecificTest.GH1327
{
	public abstract class Animal
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Box Box { get; set; }
	}

	public class Cat : Animal
	{
	}

	public class Dog : Animal
	{
	}

	public abstract class Box
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class CatBox : Box
	{
	}

	public class DogBox : Box
	{
	}
}
