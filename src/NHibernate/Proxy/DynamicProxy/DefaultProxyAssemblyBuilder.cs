using System;
using System.Reflection;
using System.Reflection.Emit;

namespace NHibernate.Proxy.DynamicProxy
{
	public class DefaultProxyAssemblyBuilder : IProxyAssemblyBuilder
	{
		public AssemblyBuilder DefineDynamicAssembly(AppDomain appDomain, AssemblyName name)
		{
#if DEBUG && !NETCOREAPP2_0
			AssemblyBuilderAccess access = AssemblyBuilderAccess.RunAndSave;
#else
			AssemblyBuilderAccess access = AssemblyBuilderAccess.Run;
#endif
#if NETCOREAPP2_0
			return AssemblyBuilder.DefineDynamicAssembly(name, access);
#else
			return appDomain.DefineDynamicAssembly(name, access);
#endif
		}

		public ModuleBuilder DefineDynamicModule(AssemblyBuilder assemblyBuilder, string moduleName)
		{
#if DEBUG && !NETCOREAPP2_0
			ModuleBuilder moduleBuilder =
				assemblyBuilder.DefineDynamicModule(moduleName, string.Format("{0}.mod", moduleName), true);
#else
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName);
#endif
			return moduleBuilder;
		}

		public void Save(AssemblyBuilder assemblyBuilder)
		{
#if DEBUG_PROXY_OUTPUT && !NETCOREAPP2_0
			assemblyBuilder.Save("generatedAssembly.dll");
#endif
		}
	}
}
