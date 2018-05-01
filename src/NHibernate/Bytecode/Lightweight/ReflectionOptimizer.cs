using System.Linq;
using System.Reflection;
using NHibernate.Properties;
using NHibernate.Util;
using Expression = System.Linq.Expressions.Expression;

namespace NHibernate.Bytecode.Lightweight
{
	public class ReflectionOptimizer : IReflectionOptimizer, IInstantiationOptimizer
	{
		private readonly IAccessOptimizer accessOptimizer;
		private readonly CreateInstanceInvoker createInstanceMethod;
		protected readonly System.Type mappedType;
		private readonly System.Type typeOfThis;

		public IAccessOptimizer AccessOptimizer
		{
			get { return accessOptimizer; }
		}

		public IInstantiationOptimizer InstantiationOptimizer
		{
			get { return this; }
		}

		public virtual object CreateInstance()
		{
			return createInstanceMethod != null ? createInstanceMethod() : null;
		}

		/// <summary>
		/// Class constructor.
		/// </summary>
		public ReflectionOptimizer(System.Type mappedType, IGetter[] getters, ISetter[] setters)
		{
			// save off references
			this.mappedType = mappedType;
			typeOfThis = mappedType.IsValueType ? mappedType.MakeByRefType() : mappedType;
			//this.getters = getters;
			//this.setters = setters;

			GetPropertyValuesInvoker getInvoker = GenerateGetPropertyValuesMethod(getters);
			SetPropertyValuesInvoker setInvoker = GenerateSetPropertyValuesMethod(setters);

			accessOptimizer = new AccessOptimizer(getInvoker, setInvoker, getters, setters);

			createInstanceMethod = CreateCreateInstanceMethod(mappedType);
		}

		/// <summary>
		/// Generates a dynamic method which creates a new instance of <paramref name="type" />
		/// when invoked.
		/// </summary>
		protected virtual CreateInstanceInvoker CreateCreateInstanceMethod(System.Type type)
		{
			if (type.IsInterface || type.IsAbstract)
				return null;

			return Expression.Lambda<CreateInstanceInvoker>(
				Expression.Convert(Expression.New(type), typeof(object))
			).Compile();
		}

		private static readonly MethodInfo GetterCallbackInvoke = ReflectHelper.GetMethod<GetterCallback>(
			g => g.Invoke(null, 0));

		/// <summary>
		/// Generates a dynamic method on the given type.
		/// </summary>
		private GetPropertyValuesInvoker GenerateGetPropertyValuesMethod(IGetter[] getters)
		{
			var o = Expression.Parameter(typeof(object), "o");
			var c = Expression.Parameter(typeof(GetterCallback), "c");

			var t = Expression.Variable(typeOfThis, "t");

			var expressions = getters.Select(
				(g, i) => g is IMemberInfoAccessor mia
					? (Expression) Expression.Convert(Expression.MakeMemberAccess(t, mia.MemberInfo), typeof(object))
					: Expression.Call(c, GetterCallbackInvoke, o, Expression.Constant(i)));

			return Expression.Lambda<GetPropertyValuesInvoker>(
				Expression.Block(
					new[] {t},
					Expression.Assign(t, Expression.Convert(o, typeOfThis)),
					Expression.NewArrayInit(typeof(object), expressions)
				),
				o,
				c
			).Compile();
		}

		private static readonly MethodInfo SetterCallbackInvoke = ReflectHelper.GetMethod<SetterCallback>(
			g => g.Invoke(null, 0, null));

		/// <summary>
		/// Generates a dynamic method on the given type.
		/// </summary>
		/// <returns></returns>
		private SetPropertyValuesInvoker GenerateSetPropertyValuesMethod(ISetter[] setters)
		{
			var o = Expression.Parameter(typeof(object), "o");
			var a = Expression.Parameter(typeof(object[]), "a");
			var c = Expression.Parameter(typeof(SetterCallback), "c");

			var t = Expression.Variable(typeOfThis, "t");

			var expression = Expression.Block(
				setters.Select(
					(g, i) => g is IMemberInfoAccessor mia
						? (Expression) Expression.Assign(
							Expression.MakeMemberAccess(t, mia.MemberInfo),
							Expression.Convert(Expression.ArrayAccess(a, Expression.Constant(i)), mia.MemberInfo.GetPropertyOrFieldType()))
						: Expression.Call(
							c,
							SetterCallbackInvoke,
							o,
							Expression.Constant(i),
							Expression.ArrayAccess(a, Expression.Constant(i))))
			);

			return Expression.Lambda<SetPropertyValuesInvoker>(
				Expression.Block(
					new[] {t},
					Expression.Assign(t, Expression.Convert(o, typeOfThis)),
					expression
				),
				o,
				a,
				c
			).Compile();
		}
	}
}
