using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1230
{
	public class Man
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Owner Owner { get; set; }
	}

	public class Owner
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<Man> Men { get; set; } = new List<Man>();
	}
}
