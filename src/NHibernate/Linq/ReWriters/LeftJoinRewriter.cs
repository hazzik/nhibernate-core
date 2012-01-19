using System.Collections.Generic;
using NHibernate.Linq.Visitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;

namespace NHibernate.Linq.ReWriters
{
    public class LeftJoinRewriter : QueryModelVisitorBase
    {
        public static void ReWrite(QueryModel queryModel)
        {
            new LeftJoinRewriter().VisitQueryModel(queryModel);
        }

        public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
        {
            var subQuery = fromClause.FromExpression as SubQueryExpression;
            if (subQuery == null)
                return;

            var subQueryModel = subQuery.QueryModel;
            if (!IsLeftJoin(subQueryModel))
                return;

            var mainFromClause = subQueryModel.MainFromClause;
            var join = NhJoinClause.Create(mainFromClause);

            var innerSelectorMapping = new QuerySourceMapping();
            innerSelectorMapping.AddMapping(fromClause, subQueryModel.SelectClause.Selector);

            queryModel.TransformExpressions(ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences(ex, innerSelectorMapping, false));

            var innerBodyClauseMapping = new QuerySourceMapping();
            innerBodyClauseMapping.AddMapping(mainFromClause, new QuerySourceReferenceExpression(@join));

            queryModel.TransformExpressions(ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences(ex, innerBodyClauseMapping, false));

            queryModel.BodyClauses.RemoveAt(index);
            queryModel.BodyClauses.Insert(index, @join);
            InsertBodyClauses(subQueryModel.BodyClauses, queryModel, index + 1);
        }

        public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
        {
            base.VisitGroupJoinClause(groupJoinClause, queryModel, index);
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
        {
        }

        public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, GroupJoinClause groupJoinClause)
        {
            var subQuery = joinClause.InnerSequence as SubQueryExpression;
            if (subQuery == null || !IsLeftJoin(subQuery.QueryModel))
                return;

            var join = LeftJoinClause.Create(joinClause);

            var innerBodyClauseMapping = new QuerySourceMapping();
            innerBodyClauseMapping.AddMapping(joinClause, new QuerySourceReferenceExpression(@join));

            queryModel.TransformExpressions(ex => ReferenceReplacingExpressionTreeVisitor.ReplaceClauseReferences(ex, innerBodyClauseMapping, false));
            groupJoinClause.JoinClause = join;
        }

        private static void InsertBodyClauses(IEnumerable<IBodyClause> bodyClauses, QueryModel destinationQueryModel, int destinationIndex)
        {
            foreach (var bodyClause in bodyClauses)
            {
                destinationQueryModel.BodyClauses.Insert(destinationIndex, bodyClause);
                ++destinationIndex;
            }
        }

        private static bool IsLeftJoin(QueryModel subQueryModel)
        {
            return subQueryModel.ResultOperators.Count == 1 &&
                   subQueryModel.ResultOperators[0] is DefaultIfEmptyResultOperator;
        }
    }
}