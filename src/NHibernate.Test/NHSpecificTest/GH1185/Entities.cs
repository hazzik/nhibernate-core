using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1185
{
	public class PurchaseOrder
	{
		public virtual int Id { get; set; }
		public virtual IList<PurchaseOrderLine> Lines { get; set; } = new List<PurchaseOrderLine>();
	}

	public class PurchaseOrderLine
	{
		public virtual int Id { get; set; }
		public virtual Product Product { get; set; }
	}

	public class Product
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}
}
