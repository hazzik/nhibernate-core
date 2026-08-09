namespace NHibernate.Test.NHSpecificTest.GH3363
{
	public class Mother
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class Child1 : Mother
	{
		public virtual Thing1 Thing { get; set; }
	}

	public class Child2 : Mother
	{
		public virtual Thing2 Thing { get; set; }
	}

	public class Thing1
	{
		public virtual string Id { get; set; }
		public virtual string Description { get; set; }
	}

	public class Thing2
	{
		public virtual string Id { get; set; }
		public virtual string Description { get; set; }
	}
}
