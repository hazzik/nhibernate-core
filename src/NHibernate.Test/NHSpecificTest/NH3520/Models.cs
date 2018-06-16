using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH3520
{
	public class Order
	{
		public virtual int ID { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<OrderLine> Lines { get; set; }

		public static Order Create(string name)
		{
			return new Order
			{
				Name = name,
				Lines = new List<OrderLine>(),
			};
		}

		public virtual Order AddLine(OrderLine orderLine)
		{
			orderLine.Order = this;
			Lines.Add(orderLine);
			return this;
		}
	}

	public class OrderLine
	{
		public virtual int ID { get; set; }
		public virtual string Name { get; set; }
		public virtual Order Order { get; set; }

		public static OrderLine Create(string name)
		{
			return new OrderLine
			{
				Name = name,
			};
		}
	}
}
