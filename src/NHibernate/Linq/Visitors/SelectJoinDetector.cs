using System.Linq.Expressions;
using NHibernate.Linq.ReWriters;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace NHibernate.Linq.Visitors
{
	internal class SelectJoinDetector : RelinqExpressionVisitor
	{
		private readonly IIsEntityDecider _isEntityDecider;
		private readonly IJoiner _joiner;
		private bool _hasIdentifier;
		private int _identifierMemberExpressionDepth;

		public SelectJoinDetector(IIsEntityDecider isEntityDecider, IJoiner joiner)
		{
			_isEntityDecider = isEntityDecider;
			_joiner = joiner;
		}

		public void Transform(SelectClause selectClause)
		{
			selectClause.TransformExpressions(Visit);
		}

		protected override Expression VisitMember(MemberExpression expression)
		{
			if (_isEntityDecider.IsIdentifier(expression.Expression.Type, expression.Member.Name))
				_hasIdentifier = true;
			else if (_hasIdentifier)
				_identifierMemberExpressionDepth++;
			
			var result = base.VisitMember(expression);
			if (_hasIdentifier)
				_identifierMemberExpressionDepth--;

			if (_isEntityDecider.IsEntity(expression.Type) &&
				(!_hasIdentifier || _identifierMemberExpressionDepth > 0) &&
				_joiner.CanAddJoin(expression))
			{
				var key = ExpressionKeyVisitor.Visit(expression, null);
				return _joiner.AddJoin(result, key);
			}

			_hasIdentifier = false;

			return result;
		}

		protected override Expression VisitSubQuery(SubQueryExpression expression)
		{
			expression.QueryModel.TransformExpressions(Visit);
			return expression;
		}
	}
}
