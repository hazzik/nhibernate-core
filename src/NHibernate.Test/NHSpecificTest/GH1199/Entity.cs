using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1199
{
	class Kpi
	{
		public virtual int Id { get; set; }
		public virtual ISet<KpiColumn> Columns { get; set; } = new HashSet<KpiColumn>();
	}

	class KpiColumn
	{
		public virtual int Id { get; set; }
		public virtual Kpi Kpi { get; set; }
	}
}
