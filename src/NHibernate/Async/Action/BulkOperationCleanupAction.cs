﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NHibernate.Engine;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using IQueryable = NHibernate.Persister.Entity.IQueryable;

namespace NHibernate.Action
{
	public partial class BulkOperationCleanupAction : IExecutable, IAfterTransactionCompletionProcess 
	{

		#region IExecutable Members

		public Task BeforeExecutionsAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				BeforeExecutions();
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task ExecuteAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				Execute();
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public async Task ExecuteAfterTransactionCompletionAsync(bool success, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			await (EvictEntityRegionsAsync(cancellationToken)).ConfigureAwait(false);
			await (EvictCollectionRegionsAsync(cancellationToken)).ConfigureAwait(false);
		}

		private Task EvictCollectionRegionsAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				if (affectedCollectionRoles != null && affectedCollectionRoles.Any())
				{
					return session.Factory.EvictCollectionAsync(affectedCollectionRoles, cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		private Task EvictEntityRegionsAsync(CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				if (affectedEntityNames != null && affectedEntityNames.Any())
				{
					return session.Factory.EvictEntityAsync(affectedEntityNames, cancellationToken);
				}
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion
	}
}
