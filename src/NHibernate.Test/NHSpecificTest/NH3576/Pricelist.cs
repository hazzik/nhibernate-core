namespace NHibernate.Test.NHSpecificTest.NH3576
{
	public class Pricelist
	{
		public long? Id { get; set; }
		public string Name { get; set; }
		public Tenant Tenant { get; set; }
		public Pricelist BasedOn { get; set; }
		public int Precision { get; set; }
		public bool IsActive { get; set; }
	}
}
