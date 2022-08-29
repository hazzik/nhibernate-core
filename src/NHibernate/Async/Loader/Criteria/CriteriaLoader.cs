﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Param;
using NHibernate.Persister.Collection;
using NHibernate.Persister.Entity;
using NHibernate.SqlCommand;
using NHibernate.Transform;
using NHibernate.Util;

namespace NHibernate.Loader.Criteria
{
	using System.Threading.Tasks;
	using System.Threading;
	internal static partial class CriteriaLoaderExtensions
	{
		/// <summary>
		/// Loads all loaders results to single typed list
		/// </summary>
		internal static async Task<List<T>> LoadAllToListAsync<T>(this IList<CriteriaLoader> loaders, ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var subresults = new List<IList>(loaders.Count);
			foreach (var l in loaders)
			{
				subresults.Add(await (l.ListAsync(session, cancellationToken)).ConfigureAwait(false));
			}

			var results = new List<T>(subresults.Sum(r => r.Count));
			foreach (var list in subresults)
			{
				ArrayHelper.AddAll(results, list);
			}
			return results;
		}
	}

	public partial class CriteriaLoader : OuterJoinLoader
	{

		public Task<IList> ListAsync(ISessionImplementor session, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<IList>(cancellationToken);
			}
			try
			{
				return ListAsync(session, translator.GetQueryParameters(), querySpaces, cancellationToken);
			}
			catch (System.Exception ex)
			{
				return Task.FromException<IList>(ex);
			}
		}

		protected override async Task<object> GetResultColumnOrRowAsync(object[] row, IResultTransformer customResultTransformer, DbDataReader rs,
													   ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			return ResolveResultTransformer(customResultTransformer)
				.TransformTuple(await (GetResultRowAsync(row, rs, session, cancellationToken)).ConfigureAwait(false), ResultRowAliases);
		}

		protected override async Task<object[]> GetResultRowAsync(object[] row, DbDataReader rs, ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] result;

			if (translator.HasProjection)
			{
				result = new object[ResultTypes.Length];

				for (int i = 0, position = 0; i < result.Length; i++)
				{
					int numColumns = ResultTypes[i].GetColumnSpan(session.Factory);

					if (numColumns > 1)
					{
						string[] typeColumnAliases = ArrayHelper.Slice(cachedProjectedColumnAliases, position, numColumns);
						result[i] = await (ResultTypes[i].NullSafeGetAsync(rs, typeColumnAliases, session, null, cancellationToken)).ConfigureAwait(false);
					}
					else
					{
						result[i] = await (ResultTypes[i].NullSafeGetAsync(rs, cachedProjectedColumnAliases[position], session, null, cancellationToken)).ConfigureAwait(false);
					}
					position += numColumns;
				}
			}
			else
			{
				result = ToResultRow(row);
			}
			return result;
		}
	}
}
