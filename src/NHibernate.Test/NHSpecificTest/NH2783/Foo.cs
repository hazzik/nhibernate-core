using System.Collections;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2783
{
	class Foo
	{
		public virtual int Id { get; set; }

		public virtual IDictionary MyProps { get; set; } = new Dictionary<string, object>();
	}
}
