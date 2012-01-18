using Remotion.Linq.Clauses.ResultOperators;

namespace NHibernate.Linq.ResultOperators
{
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public class NonAggregatingGroupBy : ClientSideTransformOperator
	{
        public NonAggregatingGroupBy(GroupResultOperator groupBy, List<LambdaExpression> predicates)
		{
		    Predicates = predicates;
		    GroupBy = groupBy;
		}

        public List<LambdaExpression> Predicates { get; private set; }

        public GroupResultOperator GroupBy { get; private set; }
	}
}