﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Engine;

namespace NHibernate.Id
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class GuidCombGenerator : IIdentifierGenerator
	{

		#region IIdentifierGenerator Members

		/// <summary>
		/// Generate a new <see cref="Guid"/> using the comb algorithm.
		/// </summary>
		/// <param name="session">The <see cref="ISessionImplementor"/> this id is being generated in.</param>
		/// <param name="obj">The entity for which the id is being generated.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>The new identifier as a <see cref="Guid"/>.</returns>
		public Task<object> GenerateAsync(ISessionImplementor session, object obj, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				return Task.FromResult<object>(Generate(session, obj));
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion
	}
}
