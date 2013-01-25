using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Parsing;

namespace NHibernate.Linq.NestedSelects
{
	using System.Reflection;

	class SelectClauseRewriter : ExpressionTreeVisitor
	{
		static readonly Expression<Func<Tuple, bool>> WherePredicate = t => !ReferenceEquals(null, t.Items[0]);

		readonly ICollection<ExpressionHolder> expressions;
		readonly Expression parameter;
		readonly System.Type resultType;
		readonly ParameterExpression values;
		readonly int tuple;
		int position;

		public SelectClauseRewriter(Expression parameter, ParameterExpression values, ICollection<ExpressionHolder> expressions, Expression expression, System.Type resultType) 
			: this(parameter, values, expressions, expression, 0)
		{
			this.resultType = resultType;
		}

		public SelectClauseRewriter(Expression parameter, ParameterExpression values, ICollection<ExpressionHolder> expressions, Expression expression, int tuple)
		{
			this.expressions = expressions;
			this.parameter = parameter;
			this.values = values;
			this.tuple = tuple;
			this.expressions.Add(new ExpressionHolder { Expression = expression, Tuple = tuple }); //ID placeholder
		}

		protected override Expression VisitMemberExpression(MemberExpression expression)
		{
			return AddAndConvertExpression(expression);
		}

		protected override Expression VisitQuerySourceReferenceExpression(QuerySourceReferenceExpression expression)
		{
			return AddAndConvertExpression(expression);
		}

		private Expression AddAndConvertExpression(Expression expression)
		{
			expressions.Add(new ExpressionHolder { Expression = expression, Tuple = tuple });

			return Expression.Convert(
				Expression.ArrayIndex(
					Expression.MakeMemberAccess(parameter,
												Tuple.ItemsField),
					Expression.Constant(++position)),
				expression.Type);
		}

		protected override Expression VisitSubQueryExpression(SubQueryExpression expression)
		{
			var selector = expression.QueryModel.SelectClause.Selector;

			var value = Expression.Parameter(typeof (Tuple), "value");

			var rewriter = new SelectClauseRewriter(value, values, expressions, null, tuple + 1);

			var resultSelector = rewriter.VisitExpression(selector);

			var where = EnumerableHelper.GetMethod("Where",
												   new[] {typeof (IEnumerable<>), typeof (Func<,>)},
												   new[] {typeof (Tuple)});

			var select = EnumerableHelper.GetMethod("Select",
													new[] {typeof (IEnumerable<>), typeof (Func<,>)},
													new[] {typeof (Tuple), selector.Type});

			//TODO: only works for collections with explicit materialization .ToList() / .ToArray() / etc
			var toList = EnumerableHelper.GetMethod("ToList",
													new[] {typeof (IEnumerable<>)},
													new[] {selector.Type});

			if (resultType != null)
			{
				return Expression.New(GetCollectionConstructor(resultType, selector.Type),
				                                      Expression.Call(select,
				                                                      Expression.Call(where, values, WherePredicate),
				                                                      Expression.Lambda(resultSelector, value)));
			}
			else
			{
				return Expression.Call(Expression.Call(toList,
				                                       Expression.Call(select,
				                                                       Expression.Call(where, values, WherePredicate),
				                                                       Expression.Lambda(resultSelector, value))),
				                       "AsReadOnly", System.Type.EmptyTypes);
			}
		}

		private ConstructorInfo GetCollectionConstructor(System.Type collectionType, System.Type elementType)
		{
			// TODO: detect collection types
			return
				typeof (HashSet<>).MakeGenericType(elementType).GetConstructor(new [] { typeof (IEnumerable<>).MakeGenericType(elementType) });
		}
	}
}