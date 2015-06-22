using NHibernate.Linq.Clauses;
using Remotion.Linq;

namespace NHibernate.Linq.Visitors
{
	public class NhQueryModelVisitorBase : QueryModelVisitorBase, INhQueryModelVisitor
	{
		public virtual void VisitNhHavingClause(NhHavingClause havingClause, QueryModel queryModel, int index)
		{
		}

		public virtual void VisitNhJoinClause(NhJoinClause joinClause, QueryModel queryModel, int index)
		{
		}

		public virtual void VisitNhWithClause(NhWithClause withClause, QueryModel queryModel, int index)
		{
		}
	}
}