using System;

namespace NHibernate.Test.NHSpecificTest.GH1251
{
	public abstract class ServiceItemBase
	{
		public virtual Guid Id { get; set; }
		public virtual decimal CurrencyId { get; set; }
	}

	public class SI_Discount : ServiceItemBase
	{
		public virtual decimal Amount { get; set; }
	}
}
