﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using NHibernate.Cache;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Impl
{
	using System.Threading.Tasks;
	using System.Threading;
	internal partial class MultipleQueriesCacheAssembler : ICacheAssembler
	{

		#region ICacheAssembler Members

		public async Task<object> DisassembleAsync(object value, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			IList srcList = (IList) value;
			var cacheable = new List<object>();
			for (int i = 0; i < srcList.Count; i++)
			{
				ICacheAssembler[] assemblers = (ICacheAssembler[]) assemblersList[i];
				IList itemList = (IList) srcList[i];
				var singleQueryCached = new List<object>();
				foreach (object objToCache in itemList)
				{
					if (assemblers.Length == 1)
					{
						singleQueryCached.Add(await (assemblers[0].DisassembleAsync(objToCache, session, owner, cancellationToken)).ConfigureAwait(false));
					}
					else
					{
						singleQueryCached.Add(await (TypeHelper.DisassembleAsync((object[]) objToCache, assemblers, null, session, null, cancellationToken)).ConfigureAwait(false));
					}
				}
				cacheable.Add(singleQueryCached);
			}
			return cacheable;
		}

		public async Task<object> AssembleAsync(object cached, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			IList srcList = (IList) cached;
			var result = new List<object>();
			for (int i = 0; i < assemblersList.Count; i++)
			{
				ICacheAssembler[] assemblers = (ICacheAssembler[]) assemblersList[i];
				IList queryFromCache = (IList) srcList[i];
				var queryResults = new List<object>();
				foreach (object fromCache in queryFromCache)
				{
					if (assemblers.Length == 1)
					{
						queryResults.Add(await (assemblers[0].AssembleAsync(fromCache, session, owner, cancellationToken)).ConfigureAwait(false));
					}
					else
					{
						queryResults.Add(await (TypeHelper.AssembleAsync((object[]) fromCache, assemblers, session, owner, cancellationToken)).ConfigureAwait(false));
					}
				}
				result.Add(queryResults);
			}
			return result;
		}

		public Task BeforeAssembleAsync(object cached, ISessionImplementor session, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				BeforeAssemble(cached, session);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion

		public async Task<IList> GetResultFromQueryCacheAsync(ISessionImplementor session, QueryParameters queryParameters,
											 ISet<string> querySpaces, IQueryCache queryCache, QueryKey key, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (!queryParameters.ForceCacheRefresh)
			{
				IList list =
					await (queryCache.GetAsync(key, queryParameters, new ICacheAssembler[] { this }, querySpaces, session, cancellationToken)).ConfigureAwait(false);
				//we had to wrap the query results in another list in order to save all
				//the queries in the same bucket, now we need to do it the other way around.
				if (list != null)
				{
					list = (IList) list[0];
				}
				return list;
			}
			return null;
		}
	}
}
