using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NHibernate.Hql.Ast;
using NHibernate.Linq.Clauses;
using NHibernate.Linq.GroupBy;
using NHibernate.Linq.GroupJoin;
using NHibernate.Linq.NestedSelects;
using NHibernate.Linq.ResultOperators;
using NHibernate.Linq.ReWriters;
using NHibernate.Linq.Visitors.ResultOperatorProcessors;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ExpressionVisitors;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.EagerFetching;

namespace NHibernate.Linq.Visitors
{
	public class QueryModelVisitor : NhQueryModelVisitorBase, INhQueryModelVisitor
	{
		public static ExpressionToHqlTranslationResults GenerateHqlQuery(QueryModel queryModel, VisitorParameters parameters, bool root)
		{
			NestedSelectRewriter.ReWrite(queryModel, parameters.SessionFactory);

			// Remove unnecessary body operators
			RemoveUnnecessaryBodyOperators.ReWrite(queryModel);

			// Merge aggregating result operators (distinct, count, sum etc) into the select clause
			MergeAggregatingResultsRewriter.ReWrite(queryModel);

			// Swap out non-aggregating group-bys
			NonAggregatingGroupByRewriter.ReWrite(queryModel);

			// Rewrite aggregate group-by statements
			AggregatingGroupByRewriter.ReWrite(queryModel);

			// Rewrite aggregating group-joins
			AggregatingGroupJoinRewriter.ReWrite(queryModel);

			// Rewrite non-aggregating group-joins
			NonAggregatingGroupJoinRewriter.ReWrite(queryModel);

			SubQueryFromClauseFlattener.ReWrite(queryModel);

			// Rewrite paging
			PagingRewriter.ReWrite(queryModel);

			// Flatten pointless subqueries
			QueryReferenceExpressionFlattener.ReWrite(queryModel);

			// Add joins for references
			AddJoinsReWriter.ReWrite(queryModel, parameters.SessionFactory);

			// Move OrderBy clauses to end
			MoveOrderByToEndRewriter.ReWrite(queryModel);

			// Give a rewriter provided by the session factory a chance to
			// rewrite the query.
			var rewriterFactory = parameters.SessionFactory.Settings.QueryModelRewriterFactory;
			if (rewriterFactory != null)
			{
				var customVisitor = rewriterFactory.CreateVisitor(parameters);
				if (customVisitor != null)
					customVisitor.VisitQueryModel(queryModel);
			}

			// rewrite any operators that should be applied on the outer query
			// by flattening out the sub-queries that they are located in
			var result = ResultOperatorRewriter.Rewrite(queryModel);

			// Identify and name query sources
			QuerySourceIdentifier.Visit(parameters.QuerySourceNamer, queryModel);

			var visitor = new QueryModelVisitor(parameters, root, queryModel) {RewrittenOperatorResult = result};
			visitor.Visit();

			return visitor._hqlTree.GetTranslation();
		}

		private readonly IntermediateHqlTree _hqlTree;
		private static readonly ResultOperatorMap ResultOperatorMap;
		private bool _serverSide = true;

		public VisitorParameters VisitorParameters { get; private set; }

		public IStreamedDataInfo CurrentEvaluationType { get; private set; }

		public IStreamedDataInfo PreviousEvaluationType { get; private set; }

		public QueryModel Model { get; private set; }

		public ResultOperatorRewriterResult RewrittenOperatorResult { get; private set; }

		static QueryModelVisitor()
		{
			// TODO - reflection to build map
			ResultOperatorMap = new ResultOperatorMap();

			ResultOperatorMap.Add<AggregateResultOperator, ProcessAggregate>();
			ResultOperatorMap.Add<AggregateFromSeedResultOperator, ProcessAggregateFromSeed>();
			ResultOperatorMap.Add<FirstResultOperator, ProcessFirst>();
			ResultOperatorMap.Add<TakeResultOperator, ProcessTake>();
			ResultOperatorMap.Add<SkipResultOperator, ProcessSkip>();
			ResultOperatorMap.Add<GroupResultOperator, ProcessGroupBy>();
			ResultOperatorMap.Add<SingleResultOperator, ProcessSingle>();
			ResultOperatorMap.Add<ContainsResultOperator, ProcessContains>();
			ResultOperatorMap.Add<NonAggregatingGroupBy, ProcessNonAggregatingGroupBy>();
			ResultOperatorMap.Add<ClientSideSelect, ProcessClientSideSelect>();
			ResultOperatorMap.Add<ClientSideSelect2, ProcessClientSideSelect2>();
			ResultOperatorMap.Add<AnyResultOperator, ProcessAny>();
			ResultOperatorMap.Add<AllResultOperator, ProcessAll>();
			ResultOperatorMap.Add<FetchOneRequest, ProcessFetchOne>();
			ResultOperatorMap.Add<FetchManyRequest, ProcessFetchMany>();
			ResultOperatorMap.Add<CacheableResultOperator, ProcessCacheable>();
			ResultOperatorMap.Add<TimeoutResultOperator, ProcessTimeout>();
			ResultOperatorMap.Add<OfTypeResultOperator, ProcessOfType>();
			ResultOperatorMap.Add<CastResultOperator, ProcessCast>();
		}

		private QueryModelVisitor(VisitorParameters visitorParameters, bool root, QueryModel queryModel)
		{
			VisitorParameters = visitorParameters;
			Model = queryModel;
			_hqlTree = new IntermediateHqlTree(root);
		}

		private void Visit()
		{
			VisitQueryModel(Model);
		}

		public override void VisitMainFromClause(MainFromClause fromClause, QueryModel queryModel)
		{
			var querySourceName = VisitorParameters.QuerySourceNamer.GetName(fromClause);
			var hqlExpressionTree = HqlGeneratorExpressionTreeVisitor.Visit(fromClause.FromExpression, VisitorParameters);

			_hqlTree.AddFromClause(_hqlTree.TreeBuilder.Range(hqlExpressionTree, _hqlTree.TreeBuilder.Alias(querySourceName)));

			// apply any result operators that were rewritten
			if (RewrittenOperatorResult != null)
			{
				CurrentEvaluationType = RewrittenOperatorResult.EvaluationType;
				foreach (ResultOperatorBase rewrittenOperator in RewrittenOperatorResult.RewrittenOperators)
				{
					VisitResultOperator(rewrittenOperator, queryModel, -1);
				}
			}

			base.VisitMainFromClause(fromClause, queryModel);
		}

		public override void VisitAdditionalFromClause(AdditionalFromClause fromClause, QueryModel queryModel, int index)
		{
			if (HandleLeftJoin(fromClause, queryModel, index)) return;

			var querySourceName = VisitorParameters.QuerySourceNamer.GetName(fromClause);

			if (fromClause.FromExpression is MemberExpression)
			{
				// It's a join
				_hqlTree.AddFromClause(
					_hqlTree.TreeBuilder.Join(
						HqlGeneratorExpressionTreeVisitor.Visit(fromClause.FromExpression, VisitorParameters).AsExpression(),
						_hqlTree.TreeBuilder.Alias(querySourceName)));
			}
			else
			{
				// TODO - exact same code as in MainFromClause; refactor this out
				_hqlTree.AddFromClause(
					_hqlTree.TreeBuilder.Range(
						HqlGeneratorExpressionTreeVisitor.Visit(fromClause.FromExpression, VisitorParameters),
						_hqlTree.TreeBuilder.Alias(querySourceName)));

			}
		}

		private void VisitNhJoinClause(string querySourceName, NhJoinClause joinClause)
		{
			var expression = HqlGeneratorExpressionTreeVisitor.Visit(joinClause.FromExpression, VisitorParameters).AsExpression();
			var alias = _hqlTree.TreeBuilder.Alias(querySourceName);

			HqlTreeNode hqlJoin;
			if (joinClause.IsInner)
			{
				hqlJoin = _hqlTree.TreeBuilder.Join(expression, @alias);
			}
			else
			{
				hqlJoin = _hqlTree.TreeBuilder.LeftJoin(expression, @alias);
			}

			foreach (var withClause in joinClause.Restrictions)
			{
				var booleanExpression = HqlGeneratorExpressionTreeVisitor.Visit(withClause.Predicate, VisitorParameters).AsBooleanExpression();
				hqlJoin.AddChild(_hqlTree.TreeBuilder.With(booleanExpression));
			}

			_hqlTree.AddFromClause(hqlJoin);
		}

		public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
		{
			PreviousEvaluationType = CurrentEvaluationType;
			CurrentEvaluationType = resultOperator.GetOutputDataInfo(PreviousEvaluationType);

			if (resultOperator is ClientSideTransformOperator)
			{
				_serverSide = false;
			}
			else
			{
				if (!_serverSide)
				{
					throw new NotSupportedException("Processing server-side result operator after doing client-side ones.  We've got the ordering wrong...");
				}
			}

			ResultOperatorMap.Process(resultOperator, this, _hqlTree);
		}

		public override void VisitSelectClause(SelectClause selectClause, QueryModel queryModel)
		{
			CurrentEvaluationType = selectClause.GetOutputDataInfo();

			var visitor = new SelectClauseVisitor(typeof (object[]), VisitorParameters);

			visitor.VisitExpression(selectClause.Selector);

			if (visitor.ProjectionExpression != null)
			{
				_hqlTree.AddItemTransformer(visitor.ProjectionExpression);
			}

			_hqlTree.AddSelectClause(_hqlTree.TreeBuilder.Select(visitor.GetHqlNodes()));

			base.VisitSelectClause(selectClause, queryModel);
		}

		public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
		{
			var visitor = new SimplifyConditionalVisitor();
			whereClause.Predicate = visitor.Visit(whereClause.Predicate);

			// Visit the predicate to build the query
			var expression = HqlGeneratorExpressionTreeVisitor.Visit(whereClause.Predicate, VisitorParameters).AsBooleanExpression();
			_hqlTree.AddWhereClause(expression);
		}

		public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
		{
			foreach (var clause in orderByClause.Orderings)
			{
				_hqlTree.AddOrderByClause(HqlGeneratorExpressionTreeVisitor.Visit(clause.Expression, VisitorParameters).AsExpression(),
				                          clause.OrderingDirection == OrderingDirection.Asc
					                          ? _hqlTree.TreeBuilder.Ascending()
					                          : (HqlDirectionStatement) _hqlTree.TreeBuilder.Descending());
			}
		}

		public override void VisitJoinClause(JoinClause joinClause, QueryModel queryModel, int index)
		{
			var equalityVisitor = new EqualityHqlGenerator(VisitorParameters);
			var whereClause = equalityVisitor.Visit(joinClause.InnerKeySelector, joinClause.OuterKeySelector);

			_hqlTree.AddWhereClause(whereClause);

			_hqlTree.AddFromClause(
				_hqlTree.TreeBuilder.Range(
					HqlGeneratorExpressionTreeVisitor.Visit(joinClause.InnerSequence, VisitorParameters),
					_hqlTree.TreeBuilder.Alias(joinClause.ItemName)));
		}

		public override void VisitGroupJoinClause(GroupJoinClause groupJoinClause, QueryModel queryModel, int index)
		{
			throw new NotImplementedException();
		}

		public override void VisitNhHavingClause(NhHavingClause havingClause, QueryModel queryModel, int index)
		{
			var visitor = new SimplifyConditionalVisitor();
			havingClause.Predicate = visitor.Visit(havingClause.Predicate);

			// Visit the predicate to build the query
			var expression = HqlGeneratorExpressionTreeVisitor.Visit(havingClause.Predicate, VisitorParameters).AsBooleanExpression();
			_hqlTree.AddHavingClause(expression);
		}

		public override void VisitNhWithClause(NhWithClause withClause, QueryModel queryModel, int index)
		{
			var visitor = new SimplifyConditionalVisitor();
			withClause.Predicate = visitor.Visit(withClause.Predicate);

			// Visit the predicate to build the query
			var expression = HqlGeneratorExpressionTreeVisitor.Visit(withClause.Predicate, VisitorParameters).AsBooleanExpression();
			_hqlTree.AddWhereClause(expression);
		}

		public override void VisitNhJoinClause(NhJoinClause joinClause, QueryModel queryModel, int index)
		{
			var querySourceName = VisitorParameters.QuerySourceNamer.GetName(joinClause);

			VisitNhJoinClause(querySourceName, joinClause);
		}

		private bool HandleLeftJoin(AdditionalFromClause fromClause, QueryModel queryModel, int index)
		{
			//return false;
			var querySourceName = VisitorParameters.QuerySourceNamer.GetName(fromClause);

			var subQuery = fromClause.FromExpression as SubQueryExpression;
			if (subQuery == null)
				return false;

			var subQueryModel = subQuery.QueryModel;
			if (!IsLeftJoin(subQueryModel))
				return false;

			var mainFromClause = subQueryModel.MainFromClause;

			var innerBodyClauseMapping = new QuerySourceMapping();
			innerBodyClauseMapping.AddMapping(mainFromClause, new QuerySourceReferenceExpression(fromClause));

			var expression = HqlGeneratorExpressionTreeVisitor.Visit(mainFromClause.FromExpression, VisitorParameters).AsExpression();

			var alias = _hqlTree.TreeBuilder.Alias(querySourceName);

			HqlTreeNode hqlJoin = _hqlTree.TreeBuilder.LeftJoin(expression, alias);

			foreach (var withClause in subQueryModel.BodyClauses.OfType<WhereClause>())
			{
				var predicate = ReferenceReplacingExpressionVisitor.ReplaceClauseReferences(withClause.Predicate, innerBodyClauseMapping, false);
				var booleanExpression = HqlGeneratorExpressionTreeVisitor.Visit(predicate, VisitorParameters).AsBooleanExpression();
				hqlJoin.AddChild(_hqlTree.TreeBuilder.With(booleanExpression));
			}

			_hqlTree.AddFromClause(hqlJoin);

			return true;
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