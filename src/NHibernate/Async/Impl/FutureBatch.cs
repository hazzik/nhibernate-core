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
using System.Linq;
using NHibernate.Transform;

namespace NHibernate.Impl
{
	using System.Threading.Tasks;
	using System.Threading;
	public abstract partial class FutureBatch<TQueryApproach, TMultiApproach>
	{

		private async Task<IList> GetResultsAsync(CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (results != null)
				return results;

			if (!session.Factory.ConnectionProvider.Driver.SupportsMultipleQueries)
			{
				var queriesResults = new List<object>();
				foreach (var query in queries)
				{
					var result = await (ListAsync(query.Query, cancellationToken)).ConfigureAwait(false);
					if (query.Future != null)
						result = query.Future.TransformList(result);
					queriesResults.Add(result);
				}

				results = queriesResults;
			}
			else
			{
				var multiApproach = CreateMultiApproach(isCacheable, cacheRegion);
				var needTransformer = false;
				foreach (var query in queries)
				{
					AddTo(multiApproach, query.Query, query.ResultType);
					if (query.Future?.ExecuteOnEval != null)
						needTransformer = true;
				}

				if (needTransformer)
					AddResultTransformer(
						multiApproach,
						new FutureResultsTransformer(queries));

				results = await (GetResultsFromAsync(multiApproach, cancellationToken)).ConfigureAwait(false);
			}

			ClearCurrentFutureBatch();
			return results;
		}

		private async Task<IEnumerable<TResult>> GetCurrentResultAsync<TResult>(int currentIndex, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return ((IList) (await (GetResultsAsync(cancellationToken)).ConfigureAwait(false))[currentIndex]).Cast<TResult>();
		}

		// 6.0 TODO: switch to abstract
		protected virtual Task<IList> ListAsync(TQueryApproach query, CancellationToken cancellationToken)
		{
			throw new NotSupportedException("This FutureBatch implementation does not support executing queries when multiple queries are not supported");
		}
		protected abstract Task<IList> GetResultsFromAsync(TMultiApproach multiApproach, CancellationToken cancellationToken);
	}
}
