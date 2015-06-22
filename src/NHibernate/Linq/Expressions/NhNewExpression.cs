using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace NHibernate.Linq.Expressions
{
	public class NhNewExpression : Expression
	{
		private readonly ReadOnlyCollection<string> _members;
		private readonly ReadOnlyCollection<Expression> _arguments;

		public NhNewExpression(IList<string> members, IList<Expression> arguments)
		{
			_members = new ReadOnlyCollection<string>(members);
			_arguments = new ReadOnlyCollection<Expression>(arguments);
		}

		public override ExpressionType NodeType
		{
			get { return (ExpressionType) NhExpressionType.New; }
		}

		public override System.Type Type
		{
			get { return base.Type; }
		}

		public ReadOnlyCollection<Expression> Arguments
		{
			get { return _arguments; }
		}

		public ReadOnlyCollection<string> Members
		{
			get { return _members; }
		}

		protected override Expression VisitChildren(ExpressionVisitor visitor)
		{
			var arguments = visitor.VisitAndConvert(Arguments, "VisitNhNew");

			return arguments != Arguments
					   ? new NhNewExpression(Members, arguments)
					   : this;
		}
	}

	public class NhStarExpression : Expression
	{
		public NhStarExpression(Expression expression)
			: base((ExpressionType)NhExpressionType.Star, expression.Type)
		{
			Expression = expression;
		}

		public Expression Expression
		{
			get;
			private set;
		}

		protected override Expression VisitChildren(ExpressionVisitor visitor)
		{
			var newExpression = visitor.Visit(Expression);

			return newExpression != Expression
					   ? new NhStarExpression(newExpression)
					   : this;
		}
	}
}