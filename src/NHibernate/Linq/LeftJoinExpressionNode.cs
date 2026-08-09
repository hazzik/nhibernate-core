using System.Linq.Expressions;
using NHibernate.Linq.Clauses;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace NHibernate.Linq
{
	/// <summary>
	/// Parses the <c>LeftJoin</c> query operator introduced by .NET 10. Relinq dispatches on the called
	/// method, so <c>LeftJoin</c> requires a node of its own, but it differs from an inner join by the
	/// emitted clause only. It therefore delegates to <see cref="JoinExpressionNode"/>, which Relinq
	/// seals, preventing deriving from it.
	/// </summary>
	internal class LeftJoinExpressionNode : MethodCallExpressionNodeBase
	{
		private readonly JoinExpressionNode _innerJoin;

		public LeftJoinExpressionNode(
			MethodCallExpressionParseInfo parseInfo,
			Expression innerSequence,
			LambdaExpression outerKeySelector,
			LambdaExpression innerKeySelector,
			LambdaExpression resultSelector)
			: base(parseInfo)
		{
			_innerJoin = new JoinExpressionNode(parseInfo, innerSequence, outerKeySelector, innerKeySelector, resultSelector);
		}

		public override Expression Resolve(
			ParameterExpression inputParameter,
			Expression expressionToBeResolved,
			ClauseGenerationContext clauseGenerationContext)
		{
			return _innerJoin.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
		}

		protected override void ApplyNodeSpecificSemantics(QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
		{
			// The delegate registers itself as the query source of the join clause, which is fine: the
			// clause references are the same whether the join is inner or outer.
			queryModel.BodyClauses.Add(new NhOuterJoinClause(_innerJoin.CreateJoinClause(clauseGenerationContext)));
			queryModel.SelectClause.Selector = _innerJoin.GetResolvedResultSelector(clauseGenerationContext);
		}
	}
}
