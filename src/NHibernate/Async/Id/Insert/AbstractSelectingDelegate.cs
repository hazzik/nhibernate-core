﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.Exceptions;
using NHibernate.Impl;
using NHibernate.SqlCommand;
using NHibernate.SqlTypes;

namespace NHibernate.Id.Insert
{
	using System.Threading.Tasks;
	using System.Threading;
	public abstract partial class AbstractSelectingDelegate : IInsertGeneratedIdentifierDelegate
	{

		#region IInsertGeneratedIdentifierDelegate Members

		public async Task<object> PerformInsertAsync(SqlCommandInfo insertSql, ISessionImplementor session, IBinder binder, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			// NH-2145: Prevent connection releases between insert and select when we cannot perform
			// them as a single statement. Retrieving id most of the time relies on using the same connection.
			session.ConnectionManager.FlushBeginning();
			try
			{
				try
				{
					// prepare and execute the insert
					var insert = await (session.Batcher.PrepareCommandAsync(insertSql.CommandType, insertSql.Text, insertSql.ParameterTypes, cancellationToken)).ConfigureAwait(false);
					try
					{
						await (binder.BindValuesAsync(insert, cancellationToken)).ConfigureAwait(false);
						await (session.Batcher.ExecuteNonQueryAsync(insert, cancellationToken)).ConfigureAwait(false);
					}
					finally
					{
						session.Batcher.CloseCommand(insert, null);
					}
				}
				catch (DbException sqle)
				{
					throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle,
													 "could not insert: " + persister.GetInfoString(), insertSql.Text);
				}

				var selectSql = SelectSQL;
				using (session.BeginProcess())
				{
					try
					{
						//fetch the generated id in a separate query
						var idSelect = await (session.Batcher.PrepareCommandAsync(CommandType.Text, selectSql, ParametersTypes, cancellationToken)).ConfigureAwait(false);
						try
						{
							await (BindParametersAsync(session, idSelect, binder, cancellationToken)).ConfigureAwait(false);
							var rs = await (session.Batcher.ExecuteReaderAsync(idSelect, cancellationToken)).ConfigureAwait(false);
							try
							{
								return await (GetResultAsync(session, rs, binder.Entity, cancellationToken)).ConfigureAwait(false);
							}
							finally
							{
								session.Batcher.CloseReader(rs);
							}
						}
						finally
						{
							session.Batcher.CloseCommand(idSelect, null);
						}
					}
					catch (DbException sqle)
					{
						throw ADOExceptionHelper.Convert(session.Factory.SQLExceptionConverter, sqle,
														 "could not retrieve generated id after insert: " + persister.GetInfoString(),
														 insertSql.Text);
					}
				}
			}
			finally
			{
				session.ConnectionManager.FlushEnding();
			}
		}

		#endregion

		/// <summary> Extract the generated key value from the given result set. </summary>
		/// <param name="session">The session </param>
		/// <param name="rs">The result set containing the generated primary key values. </param>
		/// <param name="entity">The entity being saved. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns> The generated identifier </returns>
		protected internal abstract Task<object> GetResultAsync(ISessionImplementor session, DbDataReader rs, object entity, CancellationToken cancellationToken);

		/// <summary> Bind any required parameter values into the SQL command <see cref="SelectSQL"/>. </summary>
		/// <param name="session">The session </param>
		/// <param name="ps">The prepared <see cref="SelectSQL"/> command </param>
		/// <param name="entity">The entity being saved. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		// Since 5.2
		[Obsolete("Use or override BindParameters(ISessionImplementor session, DbCommand ps, IBinder binder) instead.")]
		protected internal virtual Task BindParametersAsync(ISessionImplementor session, DbCommand ps, object entity, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				BindParameters(session, ps, entity);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		/// <summary>Bind any required parameter values into the SQL command <see cref="SelectSQL"/>.</summary>
		/// <param name="session">The session.</param>
		/// <param name="ps">The prepared <see cref="SelectSQL"/> command.</param>
		/// <param name="binder">The binder for the entity or collection being saved.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		protected internal virtual Task BindParametersAsync(ISessionImplementor session, DbCommand ps, IBinder binder, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			// 6.0 TODO: remove the call to the obsoleted method.
#pragma warning disable 618
			return BindParametersAsync(session, ps, binder.Entity, cancellationToken);
#pragma warning restore 618
		}
	}
}
