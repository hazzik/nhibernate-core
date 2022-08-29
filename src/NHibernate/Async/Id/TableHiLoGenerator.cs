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
using System.Runtime.CompilerServices;
using NHibernate.Engine;
using NHibernate.Type;
using NHibernate.Util;

namespace NHibernate.Id
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class TableHiLoGenerator : TableGenerator
	{

		#region IIdentifierGenerator Members

		/// <summary>
		/// Generate a <see cref="Int64"/> for the identifier by selecting and updating a value in a table.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
		/// <param name="obj">The entity for which the id is being generated.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>The new identifier as a <see cref="Int64"/>.</returns>
		public override async Task<object> GenerateAsync(ISessionImplementor session, object obj, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			using (await (_asyncLock.LockAsync()).ConfigureAwait(false))
			{
				if (maxLo < 1)
				{
					//keep the behavior consistent even for boundary usages
					long val = Convert.ToInt64(await (base.GenerateAsync(session, obj, cancellationToken)).ConfigureAwait(false));
					if (val == 0)
						val = Convert.ToInt64(await (base.GenerateAsync(session, obj, cancellationToken)).ConfigureAwait(false));
					return IdentifierGeneratorFactory.CreateNumber(val, returnClass);
				}
				if (lo > maxLo)
				{
					long hival = Convert.ToInt64(await (base.GenerateAsync(session, obj, cancellationToken)).ConfigureAwait(false));
					lo = (hival == 0) ? 1 : 0;
					hi = hival * (maxLo + 1);
					log.Debug("New high value: {0}", hival);
				}

				return IdentifierGeneratorFactory.CreateNumber(hi + lo++, returnClass);
			}
		}

		#endregion
	}
}
