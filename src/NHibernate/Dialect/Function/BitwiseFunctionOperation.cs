using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace NHibernate.Dialect.Function
{
	/// <summary>
	/// Treats bitwise operations as SQL function calls.
	/// </summary>
	[Serializable]
	public class BitwiseFunctionOperation : ISQLFunction
	{
		/// <summary>
		/// Creates an instance of this class using the provided function name.
		/// </summary>
		/// <param name="functionName">
		/// The bitwise function name as defined by the SQL-Dialect.
		/// </param>
		public BitwiseFunctionOperation(string functionName)
		{
			Name = functionName;
		}

		#region ISQLFunction Members

		/// <inheritdoc />
		public IType GetReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError)
		{
			return NHibernateUtil.Int64;
		}

		/// <inheritdoc />
		public virtual IType GetEffectiveReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError)
		{
			return GetReturnType(argumentTypes, mapping, throwOnError);
		}

		/// <inheritdoc />
		public string Name { get; }

		/// <inheritdoc />
		public bool HasArguments => true;

		/// <inheritdoc />
		public bool HasParenthesesIfNoArguments => true;

		/// <inheritdoc />
		public SqlString Render(IList args, ISessionFactoryImplementor factory)
		{
			var sqlBuffer = new SqlStringBuilder();

			sqlBuffer.Add(Name);
			sqlBuffer.Add("(");
			foreach (var arg in args)
			{
				// The actual second argument may be surrounded by parentesis as additional arguments.
				// They have to be ignored, otherwise it would emit "functionName(firstArg, (, secondArg, ))"
				if (IsParens(arg.ToString()))
					continue;
				if (arg is Parameter || arg is SqlString)
					sqlBuffer.AddObject(arg);
				else
					sqlBuffer.Add(arg.ToString());
				sqlBuffer.Add(", ");
			}

			sqlBuffer.RemoveAt(sqlBuffer.Count - 1);
			sqlBuffer.Add(")");

			return sqlBuffer.ToSqlString();
		}

		#endregion

		private static bool IsParens(string candidate)
		{
			return candidate == "(" || candidate == ")";
		}
	}
}
