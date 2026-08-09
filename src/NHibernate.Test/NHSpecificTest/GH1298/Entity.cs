using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace NHibernate.Test.NHSpecificTest.GH1298
{
	class PurchaseOrder
	{
		private readonly ISet<OrderLine> _orderLines = new HashSet<OrderLine>();
		public virtual int Id { get; set; }
		public virtual ISet<OrderLine> OrderLines => _orderLines;
	}

	class OrderLine
	{
		public virtual int Id { get; set; }
		public virtual PurchaseOrder PurchaseOrder { get; set; }
	}

	class OrderProjection
	{
		public PurchaseOrder PurchaseOrder { get; set; }
		public ISet<OrderLine> OrderLines { get; set; }
	}
}
