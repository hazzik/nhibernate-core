namespace NHibernate.Test.NHSpecificTest.GH1340
{
	public class Entity
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual string ModifiedByUser { get; set; }
	}
}
