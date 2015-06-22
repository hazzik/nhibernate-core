using NHibernate.Linq.Clauses;
using Remotion.Linq;

namespace NHibernate.Linq.Visitors
{
	public interface INhQueryModelVisitor: IQueryModelVisitor
	{
		void VisitNhHavingClause(NhHavingClause havingClause, QueryModel queryModel, int index);
		void VisitNhWithClause(NhWithClause withClause, QueryModel queryModel, int index);
		void VisitNhJoinClause(NhJoinClause joinClause, QueryModel queryModel, int index);
	}
}