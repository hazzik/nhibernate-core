using System.Collections;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2783
{
	class Bar
	{
		public virtual int Id { get; set; }
		
		public virtual int DynValString2 { get; set; }

		public virtual IDictionary MyProps { get; set; } = new Dictionary<string, object>();
	}
}
