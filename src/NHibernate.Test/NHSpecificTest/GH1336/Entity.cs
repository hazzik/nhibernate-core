namespace NHibernate.Test.NHSpecificTest.GH1336
{
	public class Sale
	{
		public virtual int Id { get; set; }
		public virtual string Category { get; set; }
		public virtual decimal? Amount { get; set; }
		public virtual decimal? Receita { get; set; }
	}
}
