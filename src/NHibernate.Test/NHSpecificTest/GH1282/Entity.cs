namespace NHibernate.Test.NHSpecificTest.GH1282
{
	class Person
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class PersonDetails : Person
	{
		public virtual string Address { get; set; }
	}
}
