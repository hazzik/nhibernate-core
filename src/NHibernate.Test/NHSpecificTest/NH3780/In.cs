using NHibernate.Hql.Ast;
using NHibernate.Linq;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace NHibernate.Test.NHSpecificTest.NH3780
{
    public static class ObjectExtensions
    {
        public static bool In<T>(this T @value, IEnumerable<T> values)
        {
            return values.Contains(@value);
        }

        public static bool In<T>(this T @value, params T[] values)
        {
            return values.Contains(@value);
        }

        public static bool In<T>(this T @value, IQueryable<T> values)
        {
            return values.Contains(@value);
        }

        public static bool NotIn<T>(this T @value, IEnumerable<T> values)
        {
            return !values.Contains(@value);
        }

        public static bool NotIn<T>(this T @value, params T[] values)
        {
            return !values.Contains(@value);
        }

        public static bool NotIn<T>(this T @value, IQueryable<T> values)
        {
            return !values.Contains(@value);
        }
    }
    public class InGenerator : BaseHqlGeneratorForMethod
    {
        public InGenerator()
        {
            SupportedMethods = new[]
        {
            ReflectionHelper.GetMethodDefinition(() => 
            ObjectExtensions.In(null, (IEnumerable<object>) null)),
            ReflectionHelper.GetMethodDefinition(() => 
            ObjectExtensions.In(null, (object[]) null)),
            ReflectionHelper.GetMethodDefinition(() => 
            ObjectExtensions.In<object>(null, (IQueryable<object>) null)),
            ReflectionHelper.GetMethodDefinition(() => 
            ObjectExtensions.NotIn<object>(null, (IEnumerable<object>) null)),
            ReflectionHelper.GetMethodDefinition(() => 
            ObjectExtensions.NotIn<object>(null, (object[]) null)),
            ReflectionHelper.GetMethodDefinition(() => 
        ObjectExtensions.NotIn<object>(null, (IQueryable<object>) null))
        };
        }

        public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject,
        ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder,
        IHqlExpressionVisitor visitor)
        {
            var value = visitor.Visit(arguments[0]).AsExpression();
            HqlTreeNode inClauseNode;
            var elementType = ((Expression)arguments[0]).Type;
            if (arguments[1] is ConstantExpression)
                inClauseNode = BuildFromArray(((IEnumerable<int>)((ConstantExpression)arguments[1]).Value), treeBuilder, elementType);
            else
                inClauseNode = BuildFromExpression(arguments[1], visitor);

            var inClause = inClauseNode.Children.Any() ? (HqlTreeNode)treeBuilder.In(value, inClauseNode) : treeBuilder.Equality(treeBuilder.Constant(0), treeBuilder.Constant(1));

            if (method.Name == "NotIn")
                inClause = treeBuilder.BooleanNot((HqlBooleanExpression)inClause);

            return inClause;
        }

        private HqlTreeNode BuildFromExpression(Expression expression,
                    IHqlExpressionVisitor visitor)
        {
            //TODO: check if it's a valid expression for in clause, 
            //i.e. it selects only one column
            return visitor.Visit(expression).AsExpression();
        }

        private HqlTreeNode BuildFromArray(IEnumerable values, HqlTreeBuilder treeBuilder, System.Type elementType)
        {
            if (!elementType.IsValueType && elementType != typeof(string))
                throw new ArgumentException("Only primitives and strings can be used");

            System.Type enumUnderlyingType = elementType.IsEnum ?
            Enum.GetUnderlyingType(elementType) : null;

            var variants = new List<HqlExpression>();
            foreach (var variant in values)
            {
                var val = elementType.IsEnum ? Convert.ChangeType(variant, enumUnderlyingType) : variant;
                variants.Add(treeBuilder.Constant(val));
            }
            return treeBuilder.ExpressionSubTreeHolder(variants.ToArray());
        }
    }
    public class CustomLinqGeneratorsRegistry : DefaultLinqToHqlGeneratorsRegistry
    {
        public CustomLinqGeneratorsRegistry()
        {
            RegisterGenerator(ReflectionHelper.GetMethodDefinition(() =>
                ObjectExtensions.In<object>(null, (IEnumerable<object>)null)),
                                new InGenerator());
            RegisterGenerator(ReflectionHelper.GetMethodDefinition(() =>
                ObjectExtensions.In<object>(null, (object[])null)),
                                new InGenerator());
            RegisterGenerator(ReflectionHelper.GetMethodDefinition(() =>
                ObjectExtensions.In<object>(null, (IQueryable<object>)null)),
                                new InGenerator());
            RegisterGenerator(ReflectionHelper.GetMethodDefinition(() =>
                ObjectExtensions.NotIn<object>(null, (IEnumerable<object>)null)),
                            new InGenerator());
            RegisterGenerator(ReflectionHelper.GetMethodDefinition(() =>
                ObjectExtensions.NotIn<object>(null, (object[])null)),
                                new InGenerator());
            RegisterGenerator(ReflectionHelper.GetMethodDefinition(() =>
            ObjectExtensions.NotIn<object>(null, (IQueryable<object>)null)),
                                new InGenerator());
        }
    }
}
