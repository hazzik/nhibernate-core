namespace NHibernate.Test.NHSpecificTest.GH1246
{
	public class Entity
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual JoinedComponent Data { get; set; }
	}

	public class JoinedComponent
	{
		public virtual string Value1 { get; set; }
		public virtual string Value2 { get; set; }
	}
}
