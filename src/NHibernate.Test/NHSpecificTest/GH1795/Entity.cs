namespace NHibernate.Test.NHSpecificTest.GH1795
{
	class A
	{
		public virtual int Id { get; set; }
		public virtual B B { get; set; }
	}

	class B
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
