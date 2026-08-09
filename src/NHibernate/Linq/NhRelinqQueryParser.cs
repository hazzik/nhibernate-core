using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Engine;
using NHibernate.Linq.ExpressionTransformers;
using NHibernate.Linq.Visitors;
using NHibernate.Param;
using NHibernate.Util;
using Remotion.Linq;
using Remotion.Linq.EagerFetching.Parsing;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace NHibernate.Linq
{
	public static class NhRelinqQueryParser
	{
		private static readonly QueryParser QueryParser;

		static NhRelinqQueryParser()
		{
			var transformerRegistry = ExpressionTransformerRegistry.CreateDefault();
			transformerRegistry.Register(new RemoveRedundantCast());
			transformerRegistry.Register(new SimplifyCompareTransformer());

			// If needing a compound processor for adding other processing, do not use
			// ExpressionTreeParser.CreateDefaultProcessor(transformerRegistry), it would
			// cause NH-3961 again by including a PartialEvaluatingExpressionTreeProcessor.
			// Directly instantiate a CompoundExpressionTreeProcessor instead.
			var processor = new TransformingExpressionTreeProcessor(transformerRegistry);

			var nodeTypeProvider = new NHibernateNodeTypeProvider();

			var expressionTreeParser = new ExpressionTreeParser(nodeTypeProvider, processor);
			QueryParser = new QueryParser(expressionTreeParser);
		}

		// Obsolete since v5.3
		/// <summary>
		/// Applies the minimal transformations required before parametrization,
		/// expression key computing and parsing.
		/// </summary>
		/// <param name="expression">The expression to transform.</param>
		/// <returns>The transformed expression.</returns>
		[Obsolete("Use overload with PreTransformationParameters parameter")]
		public static Expression PreTransform(Expression expression)
		{
			// In order to keep the old behavior use a DML query mode to skip detecting variables,
			// which will then generate parameters for each constant expression
			return PreTransform(expression, new PreTransformationParameters(QueryMode.Delete, null)).Expression;
		}

		/// <summary>
		/// Applies the minimal transformations required before parametrization,
		/// expression key computing and parsing.
		/// </summary>
		/// <param name="expression">The expression to transform.</param>
		/// <param name="parameters">The parameters used in the transformation process.</param>
		/// <returns><see cref="PreTransformationResult"/> that contains the transformed expression.</returns>
		public static PreTransformationResult PreTransform(Expression expression, PreTransformationParameters parameters)
		{
			parameters.EvaluatableExpressionFilter = new NhEvaluatableExpressionFilter(parameters.SessionFactory);
			parameters.QueryVariables = new Dictionary<ConstantExpression, QueryVariable>();

			var partiallyEvaluatedExpression = NhPartialEvaluatingExpressionVisitor
				.EvaluateIndependentSubtrees(expression, parameters);

			return new PreTransformationResult(
				parameters.PreTransformer.Invoke(partiallyEvaluatedExpression),
				parameters.SessionFactory,
				parameters.QueryVariables);
		}

		public static QueryModel Parse(Expression expression)
		{
			return QueryParser.GetParsedQuery(expression);
		}

		internal static Func<Expression, Expression> CreatePreTransformer(IExpressionTransformerRegistrar expressionTransformerRegistrar)
		{
			var preTransformerRegistry = new ExpressionTransformerRegistry();
			// NH-3247: must remove .Net compiler char to int conversion before
			// parameterization occurs.
			preTransformerRegistry.Register(new RemoveCharToIntConversion());
			expressionTransformerRegistrar?.Register(preTransformerRegistry);

			return new TransformingExpressionTreeProcessor(preTransformerRegistry).Process;
		}
	}

	public class NHibernateNodeTypeProvider : INodeTypeProvider
	{
		private INodeTypeProvider defaultNodeTypeProvider;

		public NHibernateNodeTypeProvider()
		{
			var methodInfoRegistry = new MethodInfoBasedNodeTypeRegistry();

			methodInfoRegistry.Register(
				new[] { ReflectHelper.FastGetMethodDefinition(EagerFetchingExtensionMethods.Fetch, default(IQueryable<object>), default(Expression<Func<object, object>>)) },
				typeof(FetchOneExpressionNode));
			methodInfoRegistry.Register(
				new[] { ReflectHelper.FastGetMethodDefinition(EagerFetchingExtensionMethods.FetchLazyProperties, default(IQueryable<object>)) },
				typeof(FetchLazyPropertiesExpressionNode));
			methodInfoRegistry.Register(
				new[] { ReflectHelper.FastGetMethodDefinition(EagerFetchingExtensionMethods.FetchMany, default(IQueryable<object>), default(Expression<Func<object, IEnumerable<object>>>)) },
				typeof(FetchManyExpressionNode));
			methodInfoRegistry.Register(
				new[] { ReflectHelper.FastGetMethodDefinition(EagerFetchingExtensionMethods.ThenFetch, default(INhFetchRequest<object, object>), default(Expression<Func<object, object>>)) },
				typeof(ThenFetchOneExpressionNode));
			methodInfoRegistry.Register(
				new[] { ReflectHelper.FastGetMethodDefinition( EagerFetchingExtensionMethods.ThenFetchMany, default(INhFetchRequest<object, object>), default(Expression<Func<object, IEnumerable<object>>>)) },
				typeof(ThenFetchManyExpressionNode));
			methodInfoRegistry.Register(
				new[]
				{
					ReflectHelper.FastGetMethodDefinition(LinqExtensionMethods.WithLock, default(IQueryable<object>), default(LockMode)),
					ReflectHelper.FastGetMethodDefinition(LinqExtensionMethods.WithLock, default(IEnumerable<object>), default(LockMode))
				}, 
				typeof(LockExpressionNode));
			methodInfoRegistry.Register(GetLeftJoinMethods(), typeof(LeftJoinExpressionNode));

			var nodeTypeProvider = ExpressionTreeParser.CreateDefaultNodeTypeProvider();
			nodeTypeProvider.InnerProviders.Add(methodInfoRegistry);
			defaultNodeTypeProvider = nodeTypeProvider;
		}

		public bool IsRegistered(MethodInfo method)
		{
			// Avoid Relinq turning IDictionary.Contains into ContainsResultOperator.  We do our own processing for that method.
			if (method.DeclaringType == typeof(IDictionary) && method.Name == "Contains")
				return false;

			return defaultNodeTypeProvider.IsRegistered(method);
		}

		public System.Type GetNodeType(MethodInfo method)
		{
			return defaultNodeTypeProvider.GetNodeType(method);
		}

		/// <summary>
		/// Gets the <c>LeftJoin</c> operators added to <see cref="Queryable"/> and <see cref="Enumerable"/>
		/// by .NET 10. They are looked up by reflection, so that they get supported whenever the running
		/// framework supplies them, whatever the framework NHibernate was built for. The overloads taking
		/// an equality comparer are left out, as NHibernate cannot honor a comparer, the same way it does
		/// not support them for <see cref="Queryable.Join{TOuter, TInner, TKey, TResult}(IQueryable{TOuter}, IEnumerable{TInner}, Expression{Func{TOuter, TKey}}, Expression{Func{TInner, TKey}}, Expression{Func{TOuter, TInner, TResult}}, IEqualityComparer{TKey})"/>.
		/// </summary>
		private static IEnumerable<MethodInfo> GetLeftJoinMethods()
		{
			return GetLeftJoinMethods(typeof(Queryable)).Concat(GetLeftJoinMethods(typeof(Enumerable)));
		}

		private static IEnumerable<MethodInfo> GetLeftJoinMethods(System.Type declaringType)
		{
			return declaringType
				.GetMethods(BindingFlags.Public | BindingFlags.Static)
				.Where(
					m => m.Name == "LeftJoin" &&
						m.IsGenericMethodDefinition &&
						m.GetGenericArguments().Length == 4 &&
						m.GetParameters().Length == 5);
		}
	}
}
