namespace NHibernate.Test.NHSpecificTest.GH1960
{
	public class Person
	{
		public virtual string Name { get; set; }
		public virtual Employee Employee { get; set; }
	}

	public class Employee
	{
		public virtual string PersonName { get; set; }
		public virtual Person Person { get; set; }
	}
}
