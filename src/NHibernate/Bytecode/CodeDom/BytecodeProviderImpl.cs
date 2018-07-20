using System;
using System.CodeDom.Compiler;
using System.Reflection;
using System.Text;
using log4net;
using Microsoft.CSharp;
using NHibernate.Property;

namespace NHibernate.Bytecode.CodeDom
{
	/// <summary>
	/// CodeDOM-based bytecode provider.
	/// </summary>
	public class BytecodeProviderImpl : IBytecodeProvider
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(BytecodeProviderImpl));

        public IReflectionOptimizer GetReflectionOptimizer(System.Type clazz, IGetter[] getters, ISetter[] setters, IGetter identifierGetter, ISetter identifierSetter)
		{
			if (clazz.IsValueType)
			{
				// Cannot create optimizer for value types - the setter method will not work.
				log.Info("Disabling reflection optimizer for value type " + clazz.FullName);
				return null;
			}
			return new Generator(clazz, getters, setters, identifierGetter, identifierSetter).CreateReflectionOptimizer();
		}

		public class Generator
		{
			private CompilerParameters cp = new CompilerParameters();
			private System.Type mappedClass;
			private IGetter[] getters;
			private ISetter[] setters;
            private IGetter identifierGetter;
            private ISetter identifierSetter;

			/// <summary>
			/// ctor
			/// </summary>
			/// <param name="mappedClass">The target class</param>
			/// <param name="setters">Array of setters (except identifier)</param>
            /// <param name="getters">Array of getters (except identifier)</param>
            public Generator(System.Type mappedClass, IGetter[] getters, ISetter[] setters, IGetter identifierGetter, ISetter identifierSetter)
			{
				this.mappedClass = mappedClass;
				this.getters = getters;
				this.setters = setters;
                this.identifierGetter = identifierGetter;
                this.identifierSetter = identifierSetter;
			}

			public IReflectionOptimizer CreateReflectionOptimizer()
			{
				try
				{
					InitCompiler();
					return Build(GenerateCode());
				}
				catch (Exception e)
				{
					log.Info("Disabling reflection optimizer for class " + mappedClass.FullName);
					log.Debug("CodeDOM compilation failed", e);
					return null;
				}
			}

			/// <summary>
			/// Set up the compiler options
			/// </summary>
			private void InitCompiler()
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("Init compiler for class " + mappedClass.FullName);
				}

				cp.GenerateInMemory = true;
				cp.TreatWarningsAsErrors = true;
#if ! DEBUG
				cp.CompilerOptions = "/optimize";
#endif

				AddAssembly(Assembly.GetExecutingAssembly().Location);

				Assembly classAssembly = mappedClass.Assembly;
				AddAssembly(classAssembly.Location);

				foreach (AssemblyName referencedName in classAssembly.GetReferencedAssemblies())
				{
					Assembly referencedAssembly = Assembly.Load(referencedName);
					AddAssembly(referencedAssembly.Location);
				}
			}

			/// <summary>
			/// Add an assembly to the list of ReferencedAssemblies
			/// required to build the class
			/// </summary>
			/// <param name="name"></param>
			private void AddAssembly(string name)
			{
				if (name.StartsWith("System.")) return;

				if (!cp.ReferencedAssemblies.Contains(name))
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("Adding referenced assembly " + name);
					}
					cp.ReferencedAssemblies.Add(name);
				}
			}

			/// <summary>
			/// Build the generated code
			/// </summary>
			/// <param name="code">Generated code</param>
			/// <returns>An instance of the generated class</returns>
			private IReflectionOptimizer Build(string code)
			{
				CodeDomProvider provider = new CSharpCodeProvider();
#if NET_2_0
				CompilerResults res = provider.CompileAssemblyFromSource(cp, new string[] {code});
#else
			ICodeCompiler compiler = provider.CreateCompiler();
			CompilerResults res = compiler.CompileAssemblyFromSource( cp, code );
#endif

				if (res.Errors.HasErrors)
				{
					log.Debug("Compiled with error:\n" + code);
					foreach (CompilerError e in res.Errors)
					{
						log.Debug(
							String.Format("Line:{0}, Column:{1} Message:{2}",
							              e.Line, e.Column, e.ErrorText)
							);
					}
					throw new InvalidOperationException(res.Errors[0].ErrorText);
				}
				else
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("Compiled ok:\n" + code);
					}
				}

				Assembly assembly = res.CompiledAssembly;
				System.Type[] types = assembly.GetTypes();
				IReflectionOptimizer optimizer = (IReflectionOptimizer) assembly.CreateInstance(types[0].FullName, false,
				                                                                                BindingFlags.CreateInstance, null,
				                                                                                new object[] {setters, getters, identifierGetter, identifierSetter},
				                                                                                null, null);

				return optimizer;
			}

			private const string header =
				"using System;\n" +
				"using NHibernate.Property;\n" +
				"namespace NHibernate.Bytecode.CodeDom {\n";

			private const string classDef =
                @"public class GetSetHelper_{0} : IReflectionOptimizer, IAccessOptimizer, IPropertyAccessOptimizer {{
					ISetter[] setters;
					IGetter[] getters;
					ISetter identifierSetter;
					IGetter identifierGetter;
					
					public GetSetHelper_{0}(ISetter[] setters, IGetter[] getters, IGetter identifierGetter, ISetter identifierSetter) {{
						this.setters = setters;
						this.getters = getters;
						this.identifierSetter = identifierSetter;
						this.identifierGetter = identifierGetter;

					}}

					public IInstantiationOptimizer InstantiationOptimizer {{
						get {{ return null; }}
					}}

					public IAccessOptimizer AccessOptimizer {{
						get {{ return this; }}
					}}

					public IPropertyAccessOptimizer IdentifierAccessOptimizer {{
						get {{ return null; }}
					}}

					";

			private const string startSetMethod =
				"public void SetPropertyValues(object obj, object[] values) {{\n" +
				"  {0} t = ({0})obj;\n";

			private const string closeSetMethod =
				"}\n";

			private const string startGetMethod =
				"public object[] GetPropertyValues(object obj) {{\n" +
				"  {0} t = ({0})obj;\n" +
				"  object[] ret = new object[{1}];\n";

			private const string closeGetMethod =
				"  return ret;\n" +
				"}\n";

            private const string startIdentifierSetMethod =
                "public void SetValue(object obj, object value) {{\n" +
                "  {0} t = ({0})obj;\n";

            private const string closeIdentifierSetMethod =
                "}\n";

            private const string startIdentifierGetMethod =
                "public object GetValue(object obj) {{\n" +
                "  {0} t = ({0})obj;\n" +
                "  object ret;\n";

            private const string closeIdentifierGetMethod =
                "  return ret;\n" +
                "}\n";


			/// <summary>
			/// Check if the get of property is public
			/// </summary>
			/// <remarks>
			/// <para>If IsPublic==true I can directly set the property</para>
			/// <para>If IsPublic==false I need to use the setter/getter</para>
			/// </remarks>
			/// <param name="propertyName"></param>
			/// <returns></returns>
			private bool IsGetPublic(string propertyName)
			{
                PropertyInfo propertyInfo = mappedClass.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if ((propertyInfo != null) && (propertyInfo.GetGetMethod() != null))
                    return true;

                return false;
			}

            /// <summary>
            /// Check if the set of property is public
            /// </summary>
            /// <remarks>
            /// <para>If IsPublic==true I can directly set the property</para>
            /// <para>If IsPublic==false I need to use the setter/getter</para>
            /// </remarks>
            /// <param name="propertyName"></param>
            /// <returns></returns>
            private bool IsSetPublic(string propertyName)
            {
                PropertyInfo propertyInfo = mappedClass.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
                if ((propertyInfo != null) && (propertyInfo.GetSetMethod() != null))
                    return true;

                return false;
            }

			/// <summary>
			/// Generate the required code
			/// </summary>
			/// <returns>C# code</returns>
			private string GenerateCode()
			{
				StringBuilder sb = new StringBuilder();

				sb.Append(header);
				sb.AppendFormat(classDef, mappedClass.FullName.Replace('.', '_').Replace("+", "__"));

                GenerateSetValuesMethod(sb);

                GenerateGetValuesMethod(sb);

                GenerateSetIdentifierMethod(sb);

                GenerateGetIdentifierMethod(sb);

				sb.Append("}\n"); // Close class
				sb.Append("}\n"); // Close namespace

				return sb.ToString();
			}

            private void GenerateGetIdentifierMethod(StringBuilder sb)
            {
                sb.AppendFormat(startIdentifierGetMethod, mappedClass.FullName.Replace('+', '.'), getters.Length);
                IGetter getter = identifierGetter;
                if (getter is BasicGetter && IsGetPublic(getter.PropertyName))
                {
                    sb.AppendFormat("  ret = t.{0};\n",getter.PropertyName);
                }
                else
                {
                    sb.AppendFormat("  ret = identifierGetter.Get(obj);\n");
                }
                sb.Append(closeIdentifierGetMethod);
            }

            private void GenerateSetIdentifierMethod(StringBuilder sb)
            {
                sb.AppendFormat(startIdentifierSetMethod, mappedClass.FullName.Replace('+', '.'));
                ISetter setter = identifierSetter;

                if (setter is BasicSetter && IsSetPublic(setter.PropertyName))
                {
                    System.Type type = identifierGetter.ReturnType;

                    if (type.IsValueType)
                    {
                        sb.AppendFormat(
                            "  t.{0} = value == null ? new {1}() : ({1})value;\n",
                            setter.PropertyName,
                            type.FullName.Replace('+', '.'));
                    }
                    else
                    {
                        sb.AppendFormat("  t.{0} = ({1})value;\n",
                                        setter.PropertyName,
                                        type.FullName.Replace('+', '.'));
                    }
                }
                else
                {
                    sb.AppendFormat("  identifierSetter.Set(obj, value);\n");
                }
                sb.Append(closeIdentifierSetMethod); // Close Set
            }

            private void GenerateGetValuesMethod(StringBuilder sb)
            {
                sb.AppendFormat(startGetMethod, mappedClass.FullName.Replace('+', '.'), getters.Length);
                for (int i = 0; i < getters.Length; i++)
                {
                    IGetter getter = getters[i];
                    if (getter is BasicGetter && IsGetPublic(getter.PropertyName))
                    {
                        sb.AppendFormat("  ret[{0}] = t.{1};\n", i, getter.PropertyName);
                    }
                    else
                    {
                        sb.AppendFormat("  ret[{0}] = getters[{0}].Get(obj);\n", i);
                    }
                }
                sb.Append(closeGetMethod);
            }

            private void GenerateSetValuesMethod(StringBuilder sb)
            {
                sb.AppendFormat(startSetMethod, mappedClass.FullName.Replace('+', '.'));
                for (int i = 0; i < setters.Length; i++)
                {
                    ISetter setter = setters[i];

                    if (setter is BasicSetter && IsSetPublic(setter.PropertyName))
                    {
                        System.Type type = getters[i].ReturnType;

                        if (type.IsValueType)
                        {
                            sb.AppendFormat(
                                "  t.{0} = values[{2}] == null ? new {1}() : ({1})values[{2}];\n",
                                setter.PropertyName,
                                type.FullName.Replace('+', '.'),
                                i);
                        }
                        else
                        {
                            sb.AppendFormat("  t.{0} = ({1})values[{2}];\n",
                                            setter.PropertyName,
                                            type.FullName.Replace('+', '.'),
                                            i);
                        }
                    }
                    else
                    {
                        sb.AppendFormat("  setters[{0}].Set(obj, values[{0}]);\n", i);
                    }
                }
                sb.Append(closeSetMethod); // Close Set
            }
		}
	}
}