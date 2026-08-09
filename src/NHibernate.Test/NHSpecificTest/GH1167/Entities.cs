namespace NHibernate.Test.NHSpecificTest.GH1167
{
	public interface INamed
	{
		int Id { get; set; }
		string Name { get; set; }
	}

	public class EntityA : INamed
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class EntityB : INamed
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
