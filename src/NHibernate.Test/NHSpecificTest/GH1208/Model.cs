using System;

namespace NHibernate.Test.NHSpecificTest.GH1208
{
	public class Invoice
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Category Category { get; set; }
	}

	public class Category
	{
		public virtual Guid Id { get; set; }
		public virtual string Name { get; set; }
		public virtual DateTime ValidUntil { get; set; }
	}
}
