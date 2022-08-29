using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Multi;
using NHibernate.Type;
using NHibernate.Util;
using Remotion.Linq.Parsing.ExpressionVisitors;
using static NHibernate.Util.ReflectionCache.QueryableMethods;

namespace NHibernate.Linq
{
	/// <summary>
	/// NHibernate LINQ extension methods. They are meant to work with <see cref="NhQueryable{T}"/>. Supplied <see cref="IQueryable{T}"/> parameters
	/// should at least have an <see cref="INhQueryProvider"/> <see cref="IQueryable.Provider"/>. <see cref="ISession.Query{T}()"/> and
	/// its overloads supply such queryables.
	/// </summary>
	public static class LinqExtensionMethods
	{
		#region AnyAsync

		/// <summary>Determines whether a sequence contains any elements.</summary>
		/// <param name="source">A sequence to check for being empty.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>true if the source sequence contains any elements; otherwise, false.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<bool>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AnyDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<bool>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<bool>(ex);
			}
		}

		/// <summary>Determines whether any element of a sequence satisfies a condition.</summary>
		/// <param name="source">A sequence whose elements to test for a condition.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>true if any elements in the source sequence pass the test in the specified predicate; otherwise, false.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<bool> AnyAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<bool>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AnyWithPredicateDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<bool>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<bool>(ex);
			}
		}

		#endregion

		#region AllAsync

		/// <summary>Determines whether all elements of a sequence satisfies a condition.</summary>
		/// <param name="source">A sequence whose elements to test for a condition.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>true if all elements in the source sequence pass the test in the specified predicate; otherwise, false.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<bool> AllAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<bool>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AllDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<bool>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<bool>(ex);
			}
		}

		#endregion

		#region CountAsync

		/// <summary>Returns the number of elements in a sequence.</summary>
		/// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>The number of elements in the input sequence.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
		public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.CountDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<int>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<int>(ex);
			}
		}

		/// <summary>Returns the number of elements in the specified sequence that satisfies a condition.</summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>The number of elements in the sequence that satisfies the condition in the predicate function.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int32.MaxValue" />.</exception>
		public static Task<int> CountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.CountWithPredicateDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<int>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<int>(ex);
			}
		}

		#endregion

		#region SumAsync

		/// <summary>
		/// Computes the sum of a sequence of <see cref="T:System.Int32"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Int32"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue"/>.</exception>
		public static Task<int> SumAsync(this IQueryable<int> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfInt;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<int>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<int>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of nullable <see cref="T:System.Int32"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Int32"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue"/>.</exception>
		public static Task<int?> SumAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfNullableInt;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<int?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<int?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of <see cref="T:System.Int64"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Int64"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue"/>.</exception>
		public static Task<long> SumAsync(this IQueryable<long> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<long>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfLong;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<long>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<long>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of nullable <see cref="T:System.Int64"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Int64"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue"/>.</exception>
		public static Task<long?> SumAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<long?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfNullableLong;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<long?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<long?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of <see cref="T:System.Single"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Single"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Single.MaxValue"/>.</exception>
		public static Task<float> SumAsync(this IQueryable<float> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfFloat;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<float>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of nullable <see cref="T:System.Single"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Single"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Single.MaxValue"/>.</exception>
		public static Task<float?> SumAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfNullableFloat;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<float?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of <see cref="T:System.Double"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Double"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Double.MaxValue"/>.</exception>
		public static Task<double> SumAsync(this IQueryable<double> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfDouble;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of nullable <see cref="T:System.Double"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Double"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Double.MaxValue"/>.</exception>
		public static Task<double?> SumAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfNullableDouble;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of <see cref="T:System.Decimal"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Decimal"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue"/>.</exception>
		public static Task<decimal> SumAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfDecimal;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<decimal>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of a sequence of nullable <see cref="T:System.Decimal"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Decimal"/> values to calculate the sum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The sum of the values in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue"/>.</exception>
		public static Task<decimal?> SumAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumOfNullableDecimal;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<decimal?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of <see cref="T:System.Int32"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue"/>.</exception>
		public static Task<int> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfIntDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<int>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<int>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of nullable <see cref="T:System.Int32"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int32.MaxValue"/>.</exception>
		public static Task<int?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfNullableIntDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<int?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<int?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of <see cref="T:System.Int64"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue"/>.</exception>
		public static Task<long> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<long>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfLongDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<long>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<long>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of nullable <see cref="T:System.Int64"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Int64.MaxValue"/>.</exception>
		public static Task<long?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<long?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfNullableLongDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<long?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<long?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of <see cref="T:System.Single"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Single.MaxValue"/>.</exception>
		public static Task<float> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfFloatDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<float>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of nullable <see cref="T:System.Single"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Single.MaxValue"/>.</exception>
		public static Task<float?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfNullableFloatDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<float?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of <see cref="T:System.Double"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Double.MaxValue"/>.</exception>
		public static Task<double> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfDoubleDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of nullable <see cref="T:System.Double"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Double.MaxValue"/>.</exception>
		public static Task<double?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfNullableDoubleDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of <see cref="T:System.Decimal"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue"/>.</exception>
		public static Task<decimal> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfDecimalDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<decimal>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal>(ex);
			}
		}

		/// <summary>
		/// Computes the sum of the sequence of nullable <see cref="T:System.Decimal"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values of type <paramref name="source"/>.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The sum of the projected values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The sum is larger than <see cref="F:System.Decimal.MaxValue"/>.</exception>
		public static Task<decimal?> SumAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SumWithSelectorOfNullableDecimalDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<decimal?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal?>(ex);
			}
		}

		#endregion

		#region Average

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Int32"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Int32"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<double> AverageAsync(this IQueryable<int> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfInt;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Int32"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Int32"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the source sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<double?> AverageAsync(this IQueryable<int?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfNullableInt;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Int64"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Int64"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<double> AverageAsync(this IQueryable<long> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfLong;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Int64"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Int64"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the source sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<double?> AverageAsync(this IQueryable<long?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfNullableLong;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Single"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Single"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<float> AverageAsync(this IQueryable<float> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfFloat;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<float>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Single"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Single"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the source sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<float?> AverageAsync(this IQueryable<float?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfNullableFloat;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<float?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Double"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Double"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<double> AverageAsync(this IQueryable<double> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfDouble;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Double"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Double"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the source sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<double?> AverageAsync(this IQueryable<double?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfNullableDouble;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Decimal"/> values.
		/// </summary>
		/// <param name="source">A sequence of <see cref="T:System.Decimal"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<decimal> AverageAsync(this IQueryable<decimal> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfDecimal;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<decimal>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Decimal"/> values.
		/// </summary>
		/// <param name="source">A sequence of nullable <see cref="T:System.Decimal"/> values to calculate the average of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the source sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<decimal?> AverageAsync(this IQueryable<decimal?> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageOfNullableDecimal;
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<decimal?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Int32"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfIntDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Int32"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the <paramref name="source"/> sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, int?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfNullableIntDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Int64"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfLongDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Int64"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the <paramref name="source"/> sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, long?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfNullableLongDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Single"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<float> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfFloatDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<float>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Single"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the <paramref name="source"/> sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<float?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, float?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<float?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfNullableFloatDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<float?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<float?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Double"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<double> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfDoubleDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Double"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the <paramref name="source"/> sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<double?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, double?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<double?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfNullableDoubleDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<double?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<double?>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of <see cref="T:System.Decimal"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException"><paramref name="source"/> contains no elements.</exception>
		public static Task<decimal> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfDecimalDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<decimal>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal>(ex);
			}
		}

		/// <summary>
		/// Computes the average of a sequence of nullable <see cref="T:System.Decimal"/> values that is obtained by invoking a projection function on each element of the input sequence.
		/// </summary>
		/// <param name="source">A sequence of values to calculate the average of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The average of the sequence of values, or <see langword="null"/> if the <paramref name="source"/> sequence is empty or contains only <see langword="null"/> values.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<decimal?> AverageAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, decimal?>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<decimal?>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.AverageWithSelectorOfNullableDecimalDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<decimal?>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<decimal?>(ex);
			}
		}

		#endregion

		#region MinAsync

		/// <summary>
		/// Returns the minimum value of a generic <see cref="T:System.Linq.IQueryable`1"/>.
		/// </summary>
		/// <param name="source">A sequence of values to determine the minimum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The minimum value in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<TSource> MinAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.MinDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		/// <summary>
		/// Invokes a projection function on each element of a generic <see cref="T:System.Linq.IQueryable`1"/> and returns the minimum resulting value.
		/// </summary>
		/// <param name="source">A sequence of values to determine the minimum of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector"/>.</typeparam>
		/// <returns>
		/// The minimum value in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<TResult> MinAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TResult>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.MinWithSelectorDefinition.MakeGenericMethod(typeof(TSource), typeof(TResult));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<TResult>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TResult>(ex);
			}
		}

		#endregion

		#region MaxAsync

		/// <summary>
		/// Returns the maximum value in a generic <see cref="T:System.Linq.IQueryable`1"/>.
		/// </summary>
		/// <param name="source">A sequence of values to determine the maximum of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <returns>
		/// The maximum value in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<TSource> MaxAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.MaxDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		/// <summary>
		/// Invokes a projection function on each element of a generic <see cref="T:System.Linq.IQueryable`1"/> and returns the maximum resulting value.
		/// </summary>
		/// <param name="source">A sequence of values to determine the maximum of.</param>
		/// <param name="selector">A projection function to apply to each element.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector"/>.</typeparam>
		/// <returns>
		/// The maximum value in the sequence.
		/// </returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source"/> or <paramref name="selector"/> is <see langword="null"/>.</exception>
		public static Task<TResult> MaxAsync<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (selector == null)
			{
				throw new ArgumentNullException(nameof(selector));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TResult>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.MaxWithSelectorDefinition.MakeGenericMethod(typeof(TSource), typeof(TResult));
				var expression = new[] { source.Expression, Expression.Quote(selector) };
				return provider.ExecuteAsync<TResult>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TResult>(ex);
			}
		}

		#endregion

		#region LongCountAsync

		/// <summary>Returns the number of elements in a sequence.</summary>
		/// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>The number of elements in the input sequence.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
		public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<long>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.LongCountDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<long>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<long>(ex);
			}
		}

		/// <summary>Returns the number of elements in the specified sequence that satisfies a condition.</summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> that contains the elements to be counted.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>The number of elements in the sequence that satisfies the condition in the predicate function.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.OverflowException">The number of elements in <paramref name="source" /> is larger than <see cref="F:System.Int64.MaxValue" />.</exception>
		public static Task<long> LongCountAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<long>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.LongCountWithPredicateDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<long>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<long>(ex);
			}
		}

		#endregion

		#region FirstAsync

		/// <summary>Returns the first element of a sequence.</summary>
		/// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return the first element of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>The first element in <paramref name="source" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException">The source sequence is empty.</exception>
		public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.FirstDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		/// <summary>Returns the first element of a sequence that satisfies a specified condition.</summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>The first element in <paramref name="source" /> that passes the test in <paramref name="predicate" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
		public static Task<TSource> FirstAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.FirstWithPredicateDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		#endregion

		#region SingleAsync

		/// <summary>Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.</summary>
		/// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return the first element of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>The single element in <paramref name="source" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException">The source sequence is empty.</exception>
		public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SingleDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		/// <summary>Returns the only element of a sequence, and throws an exception if there is not exactly one element in the sequence.</summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <returns>The single element in <paramref name="source" /> that passes the test in <paramref name="predicate" />.</returns>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		/// <exception cref="T:System.InvalidOperationException">No element satisfies the condition in <paramref name="predicate" />.-or-The source sequence is empty.</exception>
		public static Task<TSource> SingleAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SingleWithPredicateDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		#endregion

		#region SingleOrDefaultAsync

		/// <summary>Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.</summary>
		/// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return the single element of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>default(<paramref name="source" />) if <paramref name="source" /> is empty; otherwise, the single element in <paramref name="source" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SingleOrDefaultDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		/// <summary>Returns the only element of a sequence, or a default value if the sequence is empty; this method throws an exception if there is more than one element in the sequence.</summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>default(<paramref name="source" />) if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; otherwise, the single element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<TSource> SingleOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.SingleOrDefaultWithPredicateDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		#endregion

		#region FirstOrDefaultAsync

		/// <summary>Returns the first element of a sequence, or a default value if the sequence contains no elements.</summary>
		/// <param name="source">The <see cref="T:System.Linq.IQueryable`1" /> to return the first element of.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>default(<paramref name="source" />) if <paramref name="source" /> is empty; otherwise, the first element in <paramref name="source" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.FirstOrDefaultDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		/// <summary>Returns the first element of a sequence that satisfies a specified condition or a default value if no such element is found.</summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return an element from.</param>
		/// <param name="predicate">A function to test each element for a condition.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>default(<paramref name="source" />) if <paramref name="source" /> is empty or if no element passes the test specified by <paramref name="predicate" />; otherwise, the first element in <paramref name="source" /> that passes the test specified by <paramref name="predicate" />.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> or <paramref name="predicate" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<TSource> FirstOrDefaultAsync<TSource>(this IQueryable<TSource> source, Expression<Func<TSource, bool>> predicate, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (predicate == null)
			{
				throw new ArgumentNullException(nameof(predicate));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<TSource>(cancellationToken);
			}
			try
			{
				var methodInfo = ReflectionCache.QueryableMethods.FirstOrDefaultWithPredicateDefinition.MakeGenericMethod(typeof(TSource));
				var expression = new[] { source.Expression, Expression.Quote(predicate) };
				return provider.ExecuteAsync<TSource>(Expression.Call(null, methodInfo, expression), cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<TSource>(ex);
			}
		}

		#endregion

		#region ToListAsync

		/// <summary>
		/// Executes the query and returns its result as a <see cref="List{T}"/>.
		/// </summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to return a list from.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>A <see cref="List{T}"/> containing the result of the query.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static Task<List<TSource>> ToListAsync<TSource>(this IQueryable<TSource> source, CancellationToken cancellationToken = default(CancellationToken))
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<List<TSource>>(cancellationToken);
			}
			return InternalToListAsync();

			async Task<List<TSource>> InternalToListAsync()
			{
				//TODO 6.0: Replace with provider.ExecuteListAsync
				var result = provider is DefaultQueryProvider nhQueryProvider
					? await nhQueryProvider.ExecuteListAsync<TSource>(source.Expression, cancellationToken).ConfigureAwait(false)
					: await provider.ExecuteAsync<IEnumerable<TSource>>(source.Expression, cancellationToken).ConfigureAwait(false);
				return (result as List<TSource>) ?? result.ToList();
			}
		}

		#endregion

		/// <summary>
		/// Wraps the query in a deferred <see cref="IFutureEnumerable{T}"/> which enumeration will trigger a batch of all pending future queries.
		/// </summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to convert to a future query.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>A <see cref="IFutureEnumerable{T}"/>.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static IFutureEnumerable<TSource> ToFuture<TSource>(this IQueryable<TSource> source)
		{
			if (source.Provider is ISupportFutureBatchNhQueryProvider batchProvider)
			{
				return batchProvider.Session.GetFutureBatch().AddAsFuture(source);
			}
#pragma warning disable CS0618 // Type or member is obsolete
			var provider = GetNhProvider(source);
			return provider.ExecuteFuture<TSource>(source.Expression);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		/// <summary>
		/// Wraps the query in a deferred <see cref="IFutureValue{T}"/> which will trigger a batch of all pending future queries
		/// when its <see cref="IFutureValue{T}.Value"/> is read.
		/// </summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to convert to a future query.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <returns>A <see cref="IFutureValue{T}"/>.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static IFutureValue<TSource> ToFutureValue<TSource>(this IQueryable<TSource> source)
		{
			if (source.Provider is ISupportFutureBatchNhQueryProvider batchProvider)
			{
				return batchProvider.Session.GetFutureBatch().AddAsFutureValue(source);
			}
#pragma warning disable CS0618, CS0612// Type or member is obsolete
			var provider = GetNhProvider(source);
			var future = provider.ExecuteFuture<TSource>(source.Expression);
			return new FutureValue<TSource>(future.GetEnumerable, future.GetEnumerableAsync);
#pragma warning restore CS0618, CS0612// Type or member is obsolete
		}

		/// <summary>
		/// Wraps the query in a deferred <see cref="IFutureValue{T}"/> which will trigger a batch of all pending future queries
		/// when its <see cref="IFutureValue{T}.Value"/> is read.
		/// </summary>
		/// <param name="source">An <see cref="T:System.Linq.IQueryable`1" /> to convert to a future query.</param>
		/// <param name="selector">An aggregation function to apply to <paramref name="source"/>.</param>
		/// <typeparam name="TSource">The type of the elements of <paramref name="source" />.</typeparam>
		/// <typeparam name="TResult">The type of the value returned by the function represented by <paramref name="selector"/>.</typeparam>
		/// <returns>A <see cref="IFutureValue{T}"/>.</returns>
		/// <exception cref="T:System.ArgumentNullException"><paramref name="source" /> is <see langword="null"/>.</exception>
		/// <exception cref="T:System.NotSupportedException"><paramref name="source" /> <see cref="IQueryable.Provider"/> is not a <see cref="INhQueryProvider"/>.</exception>
		public static IFutureValue<TResult> ToFutureValue<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<IQueryable<TSource>, TResult>> selector)
		{
			if (source.Provider is ISupportFutureBatchNhQueryProvider batchProvider)
			{
				return batchProvider.Session.GetFutureBatch().AddAsFutureValue(source, selector);
			}
#pragma warning disable CS0618 // Type or member is obsolete
			var expression = ReplacingExpressionVisitor
				.Replace(selector.Parameters.Single(), source.Expression, selector.Body);
			var provider = GetNhProvider(source);
			return provider.ExecuteFutureValue<TResult>(expression);
#pragma warning restore CS0618 // Type or member is obsolete
		}

		#region LeftJoin

		// Code based on: https://stackoverflow.com/a/18782867
		/// <summary>
		/// Correlates the elements of two sequences based on matching keys. The default equality comparer is used to compare keys.
		/// </summary>
		/// <param name="outer">The first sequence to join.</param>
		/// <param name="inner">The sequence to join to the first sequence.</param>
		/// <param name="outerKeySelector">A dynamic function to extract the join key from each element of the first sequence.</param>
		/// <param name="innerKeySelector">A dynamic function to extract the join key from each element of the second sequence.</param>
		/// <param name="resultSelector">A dynamic function to create a result element from two matching elements.</param>
		/// <returns>An <see cref="IQueryable"/> obtained by performing a left join on two sequences.</returns>
		public static IQueryable<TResult> LeftJoin<TOuter, TInner, TKey, TResult>(
			this IQueryable<TOuter> outer,
			IQueryable<TInner> inner,
			Expression<Func<TOuter, TKey>> outerKeySelector,
			Expression<Func<TInner, TKey>> innerKeySelector,
			Expression<Func<TOuter, TInner, TResult>> resultSelector)
		{
			outer = outer ?? throw new ArgumentNullException(nameof(outer));
			inner = inner ?? throw new ArgumentNullException(nameof(inner));
			outerKeySelector = outerKeySelector ?? throw new ArgumentNullException(nameof(outerKeySelector));
			innerKeySelector = innerKeySelector ?? throw new ArgumentNullException(nameof(innerKeySelector));
			resultSelector = resultSelector ?? throw new ArgumentNullException(nameof(resultSelector));

			Expression<Func<TOuter, IEnumerable<TInner>, LeftJoinIntermediate<TOuter, TInner>>> groupJoinResultSelector =
				(oneOuter, manyInners) => new LeftJoinIntermediate<TOuter, TInner>
				{
					OneOuter = oneOuter,
					ManyInners = manyInners
				};
			var groupJoin = GroupJoinDefinition.MakeGenericMethod(
				typeof(TOuter),
				typeof(TInner),
				typeof(TKey),
				typeof(LeftJoinIntermediate<TOuter, TInner>));
			var selectMany = SelectManyDefinition.MakeGenericMethod(
				typeof(LeftJoinIntermediate<TOuter, TInner>),
				typeof(TInner),
				typeof(TResult));
			var exprGroupJoin = Expression.Call(
				groupJoin,
				outer.Expression,
				inner.Expression,
				outerKeySelector,
				innerKeySelector,
				groupJoinResultSelector);
			var selectManyCollectionSelector = (Expression<Func<LeftJoinIntermediate<TOuter, TInner>, IEnumerable<TInner>>>)
				(t => t.ManyInners.DefaultIfEmpty());
			var outerParameter = resultSelector.Parameters[0];
			var paramNew = Expression.Parameter(typeof(LeftJoinIntermediate<TOuter, TInner>));
			var outerProperty = Expression.Property(paramNew, nameof(LeftJoinIntermediate<TOuter, TInner>.OneOuter));
			var selectManyResultSelector = Expression.Lambda(
				ReplacingExpressionVisitor.Replace(outerParameter, outerProperty, resultSelector.Body),
				paramNew,
				resultSelector.Parameters[1]);

			return outer.Provider.CreateQuery<TResult>(
				Expression.Call(selectMany, exprGroupJoin, selectManyCollectionSelector, selectManyResultSelector));
		}

		private class LeftJoinIntermediate<TOuter, TInner>
		{
			public TOuter OneOuter { get; set; }
			public IEnumerable<TInner> ManyInners { get; set; }
		}

		#endregion

		/// <summary>
		/// Allows to set NHibernate query options.
		/// </summary>
		/// <typeparam name="T">The type of the queried elements.</typeparam>
		/// <param name="query">The query on which to set options.</param>
		/// <param name="setOptions">The options setter.</param>
		/// <returns>The query altered with the options.</returns>
		//Since v5.1
		[Obsolete("Please use WithOptions instead.")]
		public static IQueryable<T> SetOptions<T>(this IQueryable<T> query, Action<IQueryableOptions> setOptions)
		{
			return WithOptions(query, setOptions);
		}

		/// <summary>
		/// Allows to set NHibernate query options.
		/// </summary>
		/// <typeparam name="T">The type of the queried elements.</typeparam>
		/// <param name="query">The query on which to set options.</param>
		/// <param name="setOptions">The options setter.</param>
		/// <returns>The query altered with the options.</returns>
		public static IQueryable<T> WithOptions<T>(this IQueryable<T> query, Action<NhQueryableOptions> setOptions)
		{
			if (!(query.Provider is IQueryProviderWithOptions queryProvider))
				throw new NotSupportedException(
					$"The query.Provider does not support setting options. Please implement {nameof(IQueryProviderWithOptions)}.");

			return queryProvider.WithOptions(setOptions).CreateQuery<T>(query.Expression);
		}

		// Since v5
		[Obsolete("Please use WithOptions instead.")]
		public static IQueryable<T> Cacheable<T>(this IQueryable<T> query)
			=> query.WithOptions(o => o.SetCacheable(true));

		// Since v5
		[Obsolete("Please use WithOptions instead.")]
		public static IQueryable<T> CacheMode<T>(this IQueryable<T> query, CacheMode cacheMode)
			=> query.WithOptions(o => o.SetCacheMode(cacheMode));

		// Since v5
		[Obsolete("Please use WithOptions instead.")]
		public static IQueryable<T> CacheRegion<T>(this IQueryable<T> query, string region)
			=> query.WithOptions(o => o.SetCacheRegion(region));

		// Since v5
		[Obsolete("Please use WithOptions instead.")]
		public static IQueryable<T> Timeout<T>(this IQueryable<T> query, int timeout)
			=> query.WithOptions(o => o.SetTimeout(timeout));

		public static IQueryable<T> WithLock<T>(this IQueryable<T> query, LockMode lockMode)
		{
			var method = ReflectHelper.FastGetMethod(WithLock, query, lockMode);

			var callExpression = Expression.Call(method, query.Expression, Expression.Constant(lockMode));

			return new NhQueryable<T>(query.Provider, callExpression);
		}

		public static IEnumerable<T> WithLock<T>(this IEnumerable<T> query, LockMode lockMode)
		{
			throw new InvalidOperationException(
				"The NHibernate.Linq.LinqExtensionMethods.WithLock(IEnumerable<T>, LockMode) method can only be used in a Linq expression.");
		}

		/// <summary>
		/// Allows to specify the parameter NHibernate type to use for a literal in a queryable expression.
		/// </summary>
		/// <typeparam name="T">The type of the literal.</typeparam>
		/// <param name="parameter">The literal value.</param>
		/// <param name="type">The NHibernate type, usually obtained from <c>NHibernateUtil</c> properties.</param>
		/// <returns>The literal value.</returns>
		[NoPreEvaluation]
		public static T MappedAs<T>(this T parameter, IType type)
		{
			throw new InvalidOperationException("The method should be used inside Linq to indicate a type of a parameter");
		}

		internal static INhQueryProvider GetNhProvider<TSource>(this IQueryable<TSource> source)
		{
			if (source == null)
			{
				throw new ArgumentNullException(nameof(source));
			}
			if (!(source.Provider is INhQueryProvider provider))
			{
				throw new NotSupportedException($"Source {nameof(source.Provider)} must be a {nameof(INhQueryProvider)}");
			}
			return provider;
		}
	}
}
