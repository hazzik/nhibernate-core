using System;

namespace NHibernate.Test.NHSpecificTest.GH1982
{
	class Order
	{
		public virtual int OrderId { get; set; }
		public virtual DateTime OrderDate { get; set; }
	}

	class OrderLine
	{
		public virtual int Id { get; set; }
		public virtual Order Order { get; set; }
	}
}
