using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Hql.Ast;
using NHibernate.Linq.Visitors;

namespace NHibernate.Linq.Functions
{
	public class GetValueOrDefaultGenerator : BaseHqlGeneratorForMethod
	{
		public GetValueOrDefaultGenerator()
		{
			SupportedMethods = new[]
								   {
									   ReflectionHelper.GetMethodDefinition<byte?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<byte?>(x => x.GetValueOrDefault(default(byte))),
									   ReflectionHelper.GetMethodDefinition<sbyte?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<sbyte?>(x => x.GetValueOrDefault(default(sbyte))),
									   ReflectionHelper.GetMethodDefinition<short?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<short?>(x => x.GetValueOrDefault(default(short))),
									   ReflectionHelper.GetMethodDefinition<ushort?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<ushort?>(x => x.GetValueOrDefault(default(ushort))),
									   ReflectionHelper.GetMethodDefinition<int?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<int?>(x => x.GetValueOrDefault(default(int))),
									   ReflectionHelper.GetMethodDefinition<uint?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<uint?>(x => x.GetValueOrDefault(default(uint))),
									   ReflectionHelper.GetMethodDefinition<long?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<long?>(x => x.GetValueOrDefault(default(long))),
									   ReflectionHelper.GetMethodDefinition<ulong?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<ulong?>(x => x.GetValueOrDefault(default(ulong))),
									   ReflectionHelper.GetMethodDefinition<float?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<float?>(x => x.GetValueOrDefault(default(float))),
									   ReflectionHelper.GetMethodDefinition<double?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<double?>(x => x.GetValueOrDefault(default(double))),
									   ReflectionHelper.GetMethodDefinition<decimal?>(x => x.GetValueOrDefault()),
									   ReflectionHelper.GetMethodDefinition<decimal?>(x => x.GetValueOrDefault(default(decimal))),
								   };
		}

		public override HqlTreeNode BuildHql(MethodInfo method, Expression targetObject, ReadOnlyCollection<Expression> arguments, HqlTreeBuilder treeBuilder, IHqlExpressionVisitor visitor)
		{
			if (arguments.Count == 0)
			{
				return treeBuilder.Coalesce(visitor.Visit(targetObject).AsExpression(), treeBuilder.Constant(0));
			}

			return treeBuilder.Coalesce(visitor.Visit(targetObject).AsExpression(), visitor.Visit(arguments[0]).AsExpression());
		}
	}
}
