namespace NHibernate.Test.NHSpecificTest.GH1018
{
	public class Child
	{
		public virtual int Id { get; set; }
	}

	public class Parent : Child
	{
		public virtual Info Info { get; set; }
	}

	public class Info
	{
		public virtual int Id { get; set; }
		public virtual Parent Parent { get; set; }
	}
}
