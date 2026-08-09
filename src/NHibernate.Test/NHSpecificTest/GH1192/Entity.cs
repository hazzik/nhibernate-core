namespace NHibernate.Test.NHSpecificTest.GH1192
{
	class Parent
	{
		public virtual int Id { get; set; }
		public virtual Child NestedField { get; set; }
	}

	class Child
	{
		public virtual int Id { get; set; }
		public virtual int Value { get; set; }
	}

	class ParentDto
	{
		public virtual int Id { get; set; }
		public virtual int Field { get; set; }
	}
}
