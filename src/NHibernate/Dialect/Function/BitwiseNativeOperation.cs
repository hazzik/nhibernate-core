using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace NHibernate.Dialect.Function
{
	/// <summary>
	/// Treats bitwise operations as native operations.
	/// </summary>
	[Serializable]
	public class BitwiseNativeOperation : ISQLFunction
	{
		private readonly string _sqlOpToken;
		private readonly bool _isUnary;

		/// <summary>
		/// Creates an instance using the giving token.
		/// </summary>
		/// <param name="sqlOpToken">
		/// The operation token.
		/// </param>
		/// <remarks>
		/// Use this constructor only if the token DOES NOT represent an unary operator.
		/// </remarks>
		public BitwiseNativeOperation(string sqlOpToken)
			: this(sqlOpToken, false)
		{
		}

		/// <summary>
		/// Creates an instance using the giving token and the flag indicating if it is an unary operator.
		/// </summary>
		/// <param name="sqlOpToken">The operation token.</param>
		/// <param name="isUnary">Whether the operation is unary or not.</param>
		public BitwiseNativeOperation(string sqlOpToken, bool isUnary)
		{
			_sqlOpToken = sqlOpToken;
			_isUnary = isUnary;
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
		public string Name => null;

		/// <inheritdoc />
		public bool HasArguments => true;

		/// <inheritdoc />
		public bool HasParenthesesIfNoArguments => false;

		/// <inheritdoc />
		public SqlString Render(IList args, ISessionFactoryImplementor factory)
		{
			if (args.Count == 0)
				throw new ArgumentException("Function argument list cannot be empty", nameof(args));

			var sqlBuffer = new SqlStringBuilder();

			if (!_isUnary)
				AddToBuffer(args[0], sqlBuffer);

			sqlBuffer.Add(" ").Add(_sqlOpToken).Add(" ");
			for (var i = _isUnary ? 0 : 1; i < args.Count; i++)
			{
				AddToBuffer(args[i], sqlBuffer);
			}

			return sqlBuffer.ToSqlString();
		}

		#endregion

		private static void AddToBuffer(object arg, SqlStringBuilder buffer)
		{
			if (arg is Parameter || arg is SqlString)
				buffer.AddObject(arg);
			else
				buffer.Add(arg.ToString());
		}
	}
}
