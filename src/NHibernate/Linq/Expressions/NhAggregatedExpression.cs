using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace NHibernate.Linq.Expressions
{
	public abstract class NhAggregatedExpression : Expression
	{
		public Expression Expression { get; set; }

		protected NhAggregatedExpression(Expression expression, NhExpressionType type)
			: base((ExpressionType)type, expression.Type)
		{
			Expression = expression;
		}

		protected NhAggregatedExpression(Expression expression, System.Type expressionType, NhExpressionType type)
			: base((ExpressionType)type, expressionType)
		{
			Expression = expression;
		}

		protected override Expression VisitChildren(ExpressionVisitor visitor)
		{
			var newExpression = visitor.Visit(Expression);

			return newExpression != Expression
					   ? CreateNew(newExpression)
					   : this;
		}

		public abstract Expression CreateNew(Expression expression);
	}
}