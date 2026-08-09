namespace NHibernate.Test.NHSpecificTest.GH3563
{
	public class Entity
	{
		public virtual int Id { get; set; }
		public virtual StatusEnum Status { get; set; }
	}

	public enum StatusEnum
	{
		Active,
		Inactive
	}
}
