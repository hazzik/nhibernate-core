using System;
using System.Linq;
using System.Reflection;
using NHibernate.Util;

namespace NHibernate.Proxy
{
	public static class DefaultDynamicProxyMethodCheckerExtensions
	{
		public static bool IsProxiable(this MethodInfo method)
		{
			return !method.IsFinal
				&& (method.DeclaringType != typeof(MarshalByRefObject))
				&& !IsFinalizeMethod(method)
				&&
				(
				((method.IsPublic || method.IsFamily) && (method.IsVirtual || method.IsAbstract)) // public or protected (virtual)
				||
				(method.IsFamilyOrAssembly && (method.IsVirtual || method.IsAbstract)) // internal protected virtual
				);
		}

		public static bool ShouldBeProxiable(this MethodInfo method)
		{
			// to use only for real methods (no getter/setter)
			return method.DeclaringType != typeof(MarshalByRefObject) &&
			       !(method.DeclaringType == typeof(object) && !method.IsVirtual) &&
			       !IsDisposeMethod(method) &&
			       (method.IsPublic || method.IsAssembly || method.IsFamilyOrAssembly);
		}

		public static bool ShouldBeProxiable(this PropertyInfo propertyInfo)
		{
			if (propertyInfo == null) return true;

			var accessors = propertyInfo.GetAccessors(true);
			return accessors.Any(x => x.IsPublic || x.IsAssembly || x.IsFamilyOrAssembly);
		}

		private static bool IsDisposeMethod(MethodInfo method)
		{
			// NH-1464
			return method.Name.Equals("Dispose") && method.MemberType == MemberTypes.Method && method.GetParameters().Length == 0;
			// return method.Name.Equals("Dispose") && method.IsMethodOf(typeof(IDisposable));
		}

		private static bool IsFinalizeMethod(MethodInfo method)
		{
			return method.GetBaseDefinition() == ReflectionCache.ObjectMethods.Finalize;
		}
	}
}
