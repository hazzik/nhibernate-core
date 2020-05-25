using System.Collections;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NHibernate.Dialect.Function
{
	/// <summary>
	/// Summary description for NoArgSQLFunction.
	/// </summary>
	[Serializable]
	public class NoArgSQLFunction : ISQLFunction
	{
		public NoArgSQLFunction(string name, IType returnType)
			: this(name, returnType, true)
		{
		}

		public NoArgSQLFunction(string name, IType returnType, bool hasParenthesesIfNoArguments)
		{
			Name = name;
			FunctionReturnType = returnType;
			HasParenthesesIfNoArguments = hasParenthesesIfNoArguments;
		}

		public IType FunctionReturnType { get; protected set; }

		#region ISQLFunction Members

		/// <inheritdoc />
		public IType GetReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError)
		{
			return FunctionReturnType;
		}

		/// <inheritdoc />
		public virtual IType GetEffectiveReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError)
		{
			return GetReturnType(argumentTypes, mapping, throwOnError);
		}

		/// <inheritdoc />
		public string Name { get; protected set; }

		public bool HasArguments
		{
			get { return false; }
		}

		public bool HasParenthesesIfNoArguments { get; protected set; }

		public virtual SqlString Render(IList args, ISessionFactoryImplementor factory)
		{
			if (args.Count > 0)
			{
				throw new QueryException("function takes no arguments: " + Name);
			}

			if (HasParenthesesIfNoArguments)
			{
				return new SqlString(Name + "()");
			}

			return new SqlString(Name);
		}

		#endregion
	}
}
