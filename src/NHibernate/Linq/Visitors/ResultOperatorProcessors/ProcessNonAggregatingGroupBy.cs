using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NHibernate.Linq.ResultOperators;
using Remotion.Linq.Clauses.Expressions;

namespace NHibernate.Linq.Visitors.ResultOperatorProcessors
{
    using System.Linq;
    using Type = System.Type;

    public class ProcessNonAggregatingGroupBy : IResultOperatorProcessor<NonAggregatingGroupBy>
    {
        public void Process(NonAggregatingGroupBy resultOperator, QueryModelVisitor queryModelVisitor, IntermediateHqlTree tree)
        {
            var tSource = queryModelVisitor.Model.SelectClause.Selector.Type;
            var tKey = resultOperator.GroupBy.KeySelector.Type;
            var tElement = resultOperator.GroupBy.ElementSelector.Type;

            // Stuff in the group by that doesn't map to HQL.  Run it client-side
            var listParameter = Expression.Parameter(typeof(IEnumerable<object>), "list");

            ParameterExpression itemParam = Expression.Parameter(tSource, "item");
            Expression keySelectorSource = itemParam;

            if (tSource != SourceOf(resultOperator.GroupBy.KeySelector))
            {
                keySelectorSource = Expression.MakeMemberAccess(itemParam,
                                                                tSource.GetMember(
                                                                    ((QuerySourceReferenceExpression)
                                                                     resultOperator.GroupBy.KeySelector).ReferencedQuerySource.
                                                                        ItemName)[0]);
            }


            Expression keySelector = new GroupByKeySelectorVisitor(keySelectorSource).Visit(resultOperator.GroupBy.KeySelector);

            Expression elementSelectorSource = itemParam;

            if (tSource != SourceOf(resultOperator.GroupBy.ElementSelector))
            {
                elementSelectorSource = Expression.MakeMemberAccess(itemParam,
                                                                    tSource.GetMember(
                                                                        ((QuerySourceReferenceExpression)
                                                                         resultOperator.GroupBy.ElementSelector).ReferencedQuerySource.
                                                                            ItemName)[0]);
            }

            Expression elementSelector = new GroupByKeySelectorVisitor(elementSelectorSource).Visit(resultOperator.GroupBy.ElementSelector);

            var groupByMethod = EnumerableHelper.GetMethod("GroupBy",
                                                           new[] { typeof(IEnumerable<>), typeof(Func<,>), typeof(Func<,>) },
                                                           new[] { tSource, tKey, tElement });

            var castToItem = EnumerableHelper.GetMethod("Cast", new[] { typeof(IEnumerable) }, new[] { tSource });

            var toList = EnumerableHelper.GetMethod("ToList", new[] { typeof(IEnumerable<>) }, new[] { resultOperator.GroupBy.ItemType });

            var keySelectorExpr = Expression.Lambda(keySelector, itemParam);

            var elementSelectorExpr = Expression.Lambda(elementSelector, itemParam);

            var castToItemExpr = Expression.Call(castToItem, listParameter);

            var groupByExpr = Expression.Call(groupByMethod, castToItemExpr, keySelectorExpr, elementSelectorExpr);

            foreach (var predicate in resultOperator.Predicates)
            {
                groupByExpr = Expression.Call(typeof (Enumerable), "Where", new[] {resultOperator.GroupBy.ItemType}, groupByExpr, predicate);
            }

            var toListExpr = Expression.Call(toList, groupByExpr);

            var lambdaExpr = Expression.Lambda(toListExpr, listParameter);

            tree.AddListTransformer(lambdaExpr);
        }

        private static System.Type SourceOf(Expression keySelector)
        {
            return new GroupByKeySourceFinder().Visit(keySelector).Type;
        }
    }
}