using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1273
{
	class Operation
	{
		public virtual int Id { get; set; }
		public virtual IList<OperationStep> Steps { get; set; } = new List<OperationStep>();
	}

	class OperationStep
	{
		public virtual int Id { get; set; }
		public virtual int Order { get; set; }
	}
}
