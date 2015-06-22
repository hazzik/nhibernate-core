using System.Linq.Expressions;
using NHibernate.Linq.ReWriters;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace NHibernate.Linq.Visitors
{
	internal class ResultOperatorAndOrderByJoinDetector : RelinqExpressionVisitor
	{
		private readonly IIsEntityDecider _isEntityDecider;
		private readonly IJoiner _joiner;
		private int _memberExpressionDepth;

		public ResultOperatorAndOrderByJoinDetector(IIsEntityDecider isEntityDecider, IJoiner joiner)
		{
			_isEntityDecider = isEntityDecider;
			_joiner = joiner;
		}

		protected override Expression VisitMember(MemberExpression expression)
		{
			var isIdentifier = _isEntityDecider.IsIdentifier(expression.Expression.Type, expression.Member.Name);
			if (!isIdentifier)
				_memberExpressionDepth++;

			var result = base.VisitMember(expression);
			
			if (!isIdentifier)
				_memberExpressionDepth--;

			if (_isEntityDecider.IsEntity(expression.Type) &&
				_memberExpressionDepth > 0 &&
				_joiner.CanAddJoin(expression))
			{
				var key = ExpressionKeyVisitor.Visit(expression, null);
				return _joiner.AddJoin(result, key);
			}

			return result;
		}

		protected override Expression VisitSubQuery(SubQueryExpression expression)
		{
			expression.QueryModel.TransformExpressions(Visit);
			return expression;
		}

		public void Transform(ResultOperatorBase resultOperator)
		{
			resultOperator.TransformExpressions(Visit);
		}

		public void Transform(Ordering ordering)
		{
			ordering.TransformExpressions(Visit);
		}
	}
}