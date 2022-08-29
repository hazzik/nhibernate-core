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
using System.Data.Common;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Persister.Collection;
using NHibernate.Util;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class ArrayType : CollectionType
	{

		/// <summary>
		/// 
		/// </summary>
		/// <param name="st"></param>
		/// <param name="value"></param>
		/// <param name="index"></param>
		/// <param name="session"></param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public override Task NullSafeSetAsync(DbCommand st, object value, int index, ISessionImplementor session, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return base.NullSafeSetAsync(st, session.PersistenceContext.GetCollectionHolder(value), index, session, cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public override async Task<object> ReplaceElementsAsync(object original, object target, object owner, IDictionary copyCache, ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			Array org = (Array) original;
			Array result = (Array) target;

			int length = org.Length;
			if (length != result.Length)
			{
				//note: this affects the return value!
				result = (Array) InstantiateResult(original);
			}

			IType elemType = GetElementType(session.Factory);
			for (int i = 0; i < length; i++)
			{
				result.SetValue(await (elemType.ReplaceAsync(org.GetValue(i), null, session, owner, copyCache, cancellationToken)).ConfigureAwait(false), i);
			}

			return result;
		}
	}
}
