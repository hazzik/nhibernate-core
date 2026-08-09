namespace NHibernate.Test.NHSpecificTest.GH1087
{
	public class Person
	{
		public virtual int Id { get; set; }
		public virtual Employee Employee { get; set; }
	}

	public class Employee
	{
		public virtual int Id { get; set; }
	}
}
