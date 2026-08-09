using System;

namespace NHibernate.Test.NHSpecificTest.GH1123
{
	class OrderEntity
	{
		public virtual int Id { get; set; }
		public virtual DateTime ShippingDate { get; set; }
	}
}
