namespace NHibernate.Test.NHSpecificTest.NH2380
{
	public class Person
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual int Value { get; set; }
	}

	public class PersonDto
	{
		public string Name { get; set; }
		public virtual int Value { get; set; }
	}
}
