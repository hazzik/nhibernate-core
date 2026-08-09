using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1281
{
	class Customer
	{
		private readonly IList<Address> _addresses = new List<Address>();

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<Address> Addresses => _addresses;
	}

	class Address
	{
		public virtual int Id { get; set; }
		public virtual string Street { get; set; }
		public virtual Customer Customer { get; set; }
	}
}
