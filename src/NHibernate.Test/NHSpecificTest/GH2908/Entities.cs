namespace NHibernate.Test.NHSpecificTest.GH2908
{
	public interface IPerson
	{
	}

	public abstract class Person : IPerson
	{
		public virtual int Id { get; set; }
	}

	public class Programmer : Person, IPerson
	{
	}
	
	public class Manager : Person, IPerson
	{
	}

	public class Group
	{
		public virtual int Id { get; set; }

		public virtual IPerson Leader { get; set; }
	}
}
