using System.Collections;
using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.SqlCommand;
using NHibernate.Type;

namespace NHibernate.Dialect.Function
{
	/// <summary>
	/// Provides support routines for the HQL functions as used
	/// in the various SQL Dialects
	///
	/// Provides an interface for supporting various HQL functions that are
	/// translated to SQL. The Dialect and its sub-classes use this interface to
	/// provide details required for processing of the function.
	/// </summary>
	public interface ISQLFunction
	{
		/// <summary>
		/// Does this function have any arguments?
		/// </summary>
		bool HasArguments { get; }

		/// <summary>
		/// If there are no arguments, are parens required?
		/// </summary>
		bool HasParenthesesIfNoArguments { get; }

		/// <summary>
		/// The function name or <see langword="null"/> when multiple functions/operators/statements are used.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// Render the function call as SQL.
		/// </summary>
		/// <param name="args">List of arguments</param>
		/// <param name="factory"></param>
		/// <returns>SQL fragment for the function.</returns>
		SqlString Render(IList args, ISessionFactoryImplementor factory);

		/// <summary>
		/// Get the type that will be effectively returned by the underlying database.
		/// </summary>
		/// <param name="argumentTypes">The types of arguments.</param>
		/// <param name="mapping">The mapping for retrieving the argument sql types.</param>
		/// <param name="throwOnError">Whether to throw when the number of arguments is invalid or they are not supported.</param>
		/// <returns>The type returned by the underlying database or <see langword="null"/> when the number of arguments
		/// is invalid or they are not supported.</returns>
		/// <exception cref="QueryException">When <paramref name="throwOnError"/> is set to <see langword="true"/> and the
		/// number of arguments is invalid or they are not supported.</exception>
		IType GetEffectiveReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError);

		/// <summary>
		/// Get the function general return type, ignoring underlying database specifics.
		/// </summary>
		/// <param name="argumentTypes">The types of arguments.</param>
		/// <param name="mapping">The mapping for retrieving the argument sql types.</param>
		/// <param name="throwOnError">Whether to throw when the number of arguments is invalid or they are not supported.</param>
		/// <returns>The type returned by the underlying database or <see langword="null"/> when the number of arguments
		/// is invalid or they are not supported.</returns>
		IType GetReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError);
	}
}
