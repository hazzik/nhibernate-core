using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1070
{
	public class A
	{
		public virtual Guid Id { get; set; }
		public virtual ICollection<B> BCollection { get; set; } = new List<B>();
	}

	public class B
	{
		public virtual Guid Id { get; set; }
	}
}
