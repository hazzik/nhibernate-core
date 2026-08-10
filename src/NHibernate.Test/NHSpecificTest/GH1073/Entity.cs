namespace NHibernate.Test.NHSpecificTest.GH1073
{
	class Person
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class Employee : Person
	{
		public virtual string Title { get; set; }
	}
}
