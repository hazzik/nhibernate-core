using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2810
{
	public class ClassB
	{
		public virtual int B_ID { get; set; }
		public virtual string Text { get; set; }

		public virtual IList<ClassA> classA_list { get; set; }
	}
}
