using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1307
{
	class Shipper
	{
		public virtual int ShipperId { get; set; }
		public virtual string CompanyName { get; set; }
		public virtual IList<Order> Orders { get; set; } = new List<Order>();
	}

	class Order
	{
		public virtual int OrderId { get; set; }
		public virtual Shipper Shipper { get; set; }
		public virtual IList<OrderLine> OrderLines { get; set; } = new List<OrderLine>();
	}

	class OrderLine
	{
		public virtual int OrderLineId { get; set; }
		public virtual Order Order { get; set; }
		public virtual string ProductName { get; set; }
	}
}
