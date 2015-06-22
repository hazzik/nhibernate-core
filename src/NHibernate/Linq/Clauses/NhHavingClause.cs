using System;
using System.Linq.Expressions;
using NHibernate.Linq.Visitors;
using Remotion.Linq;
using Remotion.Linq.Clauses;

namespace NHibernate.Linq.Clauses
{
	public class NhHavingClause : IBodyClause
	{
		private Expression _predicate;

		public NhHavingClause(Expression predicate)
		{
			ArgumentUtility.CheckNotNull("predicate", predicate);
			_predicate = predicate;
		}

		/// <summary>
		///     Gets the predicate, the expression representing the where condition by which the data items are filtered
		/// </summary>
		public Expression Predicate
		{
			get { return _predicate; }
			set { _predicate = ArgumentUtility.CheckNotNull("value", value); }
		}

		IBodyClause IBodyClause.Clone(CloneContext cloneContext)
		{
			return Clone(cloneContext);
		}

		/// <summary>
		///     Accepts the specified visitor by calling its
		///     <see
		///         cref="M:Remotion.Linq.IQueryModelVisitor.VisitWhereClause(Remotion.Linq.Clauses.WhereClause,Remotion.Linq.QueryModel,System.Int32)" />
		///     method.
		/// </summary>
		/// <param name="visitor">The visitor to accept.</param>
		/// <param name="queryModel">The query model in whose context this clause is visited.</param>
		/// <param name="index">
		///     The index of this clause in the <paramref name="queryModel" />'s
		///     <see cref="P:Remotion.Linq.QueryModel.BodyClauses" /> collection.
		/// </param>
		public void Accept(IQueryModelVisitor visitor, QueryModel queryModel, int index)
		{
			ArgumentUtility.CheckNotNull("visitor", visitor);
			ArgumentUtility.CheckNotNull("queryModel", queryModel);
			((INhQueryModelVisitor)visitor).VisitNhHavingClause(this, queryModel, index);
		}

		/// <summary>
		///     Transforms all the expressions in this clause and its child objects via the given
		///     <paramref name="transformation" /> delegate.
		/// </summary>
		/// <param name="transformation">
		///     The transformation object. This delegate is called for each <see cref="T:System.Linq.Expressions.Expression" />
		///     within this
		///     clause, and those expressions will be replaced with what the delegate returns.
		/// </param>
		public void TransformExpressions(Func<Expression, Expression> transformation)
		{
			ArgumentUtility.CheckNotNull("transformation", transformation);
			Predicate = transformation(Predicate);
		}

		public override string ToString()
		{
			return "having " + Predicate;
		}

		/// <summary>
		///     Clones this clause.
		/// </summary>
		/// <param name="cloneContext">
		///     The clones of all query source clauses are registered with this
		///     <see cref="T:Remotion.Linq.Clauses.CloneContext" />.
		/// </param>
		/// <returns />
		public NhHavingClause Clone(CloneContext cloneContext)
		{
			ArgumentUtility.CheckNotNull("cloneContext", cloneContext);
			return new NhHavingClause(Predicate);
		}
	}
}