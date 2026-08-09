using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1346
{
	public class Abc
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<ArraySize> ArraySizes { get; set; } = new List<ArraySize>();
	}

	public class ArraySize
	{
		public virtual int Id { get; set; }
		public virtual int Size { get; set; }
	}
}
