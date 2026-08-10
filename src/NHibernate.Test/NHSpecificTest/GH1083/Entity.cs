namespace NHibernate.Test.NHSpecificTest.GH1083
{
	// The entity is deliberately named after the HQL keyword "order" (used in "order by"),
	// which is the trigger for the reported bug: NH-2562.
	public class Order
	{
		public virtual int Id { get; set; }
		public virtual bool IsConfirmed { get; set; }
	}
}
