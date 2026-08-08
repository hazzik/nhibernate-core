using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1472
{
	public class Customer
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Contact BillingContact { get; set; }
		public virtual ISet<Purchase> Purchases { get; set; } = new HashSet<Purchase>();
	}

	public class Contact
	{
		public virtual Guid Id { get; set; }
		public virtual DateTime DateOfBirth { get; set; }
	}

	public class Purchase
	{
		public virtual Guid Id { get; set; }
		public virtual Customer Customer { get; set; }
		public virtual Contact DeliveryContact { get; set; }
	}
}
