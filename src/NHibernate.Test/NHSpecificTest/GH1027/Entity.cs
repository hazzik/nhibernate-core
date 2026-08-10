using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1027
{
	public class TextResource
	{
		public virtual int Id { get; set; }
		public virtual IDictionary<string, Translation> Translations { get; set; } = new Dictionary<string, Translation>();
	}

	public class Translation
	{
		public virtual int Id { get; set; }
		public virtual string TextValue { get; set; }
	}
}
