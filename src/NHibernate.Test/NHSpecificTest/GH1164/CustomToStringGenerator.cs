using System;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq.Functions;
using NHibernate.Linq.Visitors;

namespace NHibernate.Test.NHSpecificTest.GH1164
{
	// A LINQ generator registered specifically for MyEntity.ToString(), as opposed to the
	// generic, built-in ToStringRuntimeMethodHqlGenerator that handles object.ToString().
	public class CustomToStringGenerator : BaseHqlGeneratorForMethod
	{
		public const string Marker = "custom-marker";

		public CustomToStringGenerator()
		{
			SupportedMethods = new[] {typeof(MyEntity).GetMethod(nameof(MyEntity.ToString), System.Type.EmptyTypes)};
		}

		public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
		{
			return treeBuilder.Constant(Marker);
		}
	}
}
