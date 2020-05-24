using System.Linq.Expressions;
using System.Reflection;
using NHibernate.Util;
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;

namespace NHibernate.Linq.ExpressionTransformers
{
	/// <summary>
	/// Replace <see cref="string.StartsWith(string)"/>, <see cref="string.EndsWith(string)"/> and <see cref="string.Contains(string)"/>
	/// with <see cref="SqlMethods.Like(string, string)"/>
	/// </summary>
	internal class LikeTransformer : IExpressionTransformer<MethodCallExpression>
	{
		private static readonly MethodInfo Like = ReflectHelper.FastGetMethod(SqlMethods.Like, default(string), default(string));
		private static readonly MethodInfo LikeEscape = ReflectHelper.FastGetMethod(SqlMethods.Like, default(string), default(string), default(char));

		public ExpressionType[] SupportedExpressionTypes { get; } = {ExpressionType.Call};

		public Expression Transform(MethodCallExpression expression)
		{
			if (IsLike(expression, out var value, out var shouldEscape))
			{
				if (shouldEscape)
				{
					return Expression.Call(
						LikeEscape,
						expression.Object,
						Expression.Constant(value),
						Expression.Constant('\\')
					);
				}
				else
				{
					return Expression.Call(
						Like,
						expression.Object,
						Expression.Constant(value)
					);
				}
			}

			return expression;
		}

		private static bool IsLike(MethodCallExpression expression, out string value, out bool shouldEscape)
		{
			if (expression.Method == ReflectionCache.StringMethods.StartsWith)
			{
				if (expression.Arguments[0] is ConstantExpression constantExpression)
				{
					shouldEscape = ShouldEscape(constantExpression, out var val);
					value = string.Concat(val, "%");
					return true;
				}
			}
			else if (expression.Method == ReflectionCache.StringMethods.EndsWith)
			{
				if (expression.Arguments[0] is ConstantExpression constantExpression)
				{
					shouldEscape = ShouldEscape(constantExpression, out var val);
					value = string.Concat("%", val);
					return true;
				}
			}
			else if (expression.Method == ReflectionCache.StringMethods.Contains)
			{
				if (expression.Arguments[0] is ConstantExpression constantExpression)
				{
					shouldEscape = ShouldEscape(constantExpression, out var val);
					value = string.Concat("%", val, "%");
					return true;
				}
			}

			value = null;
			shouldEscape = false;
			return false;
		}

		private static bool ShouldEscape(ConstantExpression constantExpression, out string val)
		{
			var v = (string) constantExpression.Value;

			val = v?.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_");
			return val != v;
		}
	}
}
