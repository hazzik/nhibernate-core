using System;

namespace NHibernate.Test.NHSpecificTest.NH3373
{
	public class Company
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual AddressComponent Address { get; set; }
	}

	public class AddressComponent
	{
		public virtual string Street { get; set; }
		public virtual Country Country { get; set; }
	}

	public class Country
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
	}
}
