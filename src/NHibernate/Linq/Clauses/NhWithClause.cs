using System;
using System.Linq.Expressions;
using NHibernate.Linq.Visitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace NHibernate.Linq.Clauses
{
	public class NhWithClause : IBodyClause
	{
		public NhWithClause(Expression predicate)
		{
			ArgumentUtility.CheckNotNull("predicate", predicate);
			_predicate = predicate;
		}

		public override string ToString()
		{
			return "withClause " + Predicate;
		}

		private Expression _predicate;

		/// <summary>
		/// Gets the predicate, the expression representing the where condition by which the data items are filtered
		/// 
		/// </summary>
		public Expression Predicate
		{
			get { return _predicate; }
			set { _predicate = ArgumentUtility.CheckNotNull("value", value); }
		}

		public void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
		{
			ArgumentUtility.CheckNotNull("visitor", visitor);
			ArgumentUtility.CheckNotNull("queryModel", queryModel);
			((INhQueryModelVisitor)visitor).VisitNhWithClause(this, queryModel, index);
		}

		/// <summary>
		/// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
		/// 
		/// </summary>
		/// <param name="transformation">The transformation object. This delegate is called for each <see cref="T:System.Linq.Expressions.Expression"/> within this
		///             clause, and those expressions will be replaced withClause what the delegate returns.</param>
		public void TransformExpressions(Func<Expression, Expression> transformation)
		{
			ArgumentUtility.CheckNotNull("transformation", transformation);
			Predicate = transformation(Predicate);
		}

		/// <summary>
		/// Clones this clause.
		/// 
		/// </summary>
		/// <param name="cloneContext">The clones of all query source clauses are registered withClause this <see cref="T:Remotion.Linq.Clauses.CloneContext"/>.</param>
		/// <returns/>
		public NhWithClause Clone(CloneContext cloneContext)
		{
			ArgumentUtility.CheckNotNull("cloneContext", cloneContext);
			return new NhWithClause(Predicate);
		}

		IBodyClause IBodyClause.Clone(CloneContext cloneContext)
		{
			return Clone(cloneContext);
		}
	}
}
