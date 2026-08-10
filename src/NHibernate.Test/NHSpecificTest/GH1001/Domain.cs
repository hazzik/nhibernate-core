using System;

namespace NHibernate.Test.NHSpecificTest.GH1001
{
	public class WidgetKey
	{
		public virtual int Number { get; set; }
		public virtual string Code { get; set; }

		public override bool Equals(object obj)
		{
			return obj is WidgetKey other && Number == other.Number && Code == other.Code;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Number, Code);
		}
	}

	public class Widget
	{
		public virtual WidgetKey Id { get; set; }
		public virtual string Description { get; set; }
	}
}
