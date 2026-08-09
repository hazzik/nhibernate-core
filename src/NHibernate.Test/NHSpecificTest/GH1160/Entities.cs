using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1160
{
	class Basket
	{
		private readonly ISet<BasketLine> _lines = new HashSet<BasketLine>();

		public virtual Guid Id { get; set; }

		public virtual string Name { get; set; }

		public virtual ISet<BasketLine> Lines => _lines;
	}

	class BasketLine
	{
		public virtual Guid Id { get; set; }

		public virtual string Description { get; set; }
	}
}
