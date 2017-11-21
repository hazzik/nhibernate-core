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
using NHibernate.Collection;
using NHibernate.Event;
using NHibernate.Persister.Entity;
using NHibernate.Proxy;
using NHibernate.Type;

namespace NHibernate.Engine
{
	using System.Threading.Tasks;
	using System.Threading;
	public abstract partial class CascadingAction
	{

		#region The CascadingAction contract

		/// <summary> Cascade the action to the child object. </summary>
		/// <param name="session">The session within which the cascade is occurring. </param>
		/// <param name="child">The child to which cascading should be performed. </param>
		/// <param name="entityName">The child's entity name </param>
		/// <param name="anything">Typically some form of cascade-local cache which is specific to each CascadingAction type </param>
		/// <param name="isCascadeDeleteEnabled">Are cascading deletes enabled. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public abstract Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken);

		/// <summary> 
		/// Called (in the case of <see cref="RequiresNoCascadeChecking"/> returning true) to validate
		/// that no cascade on the given property is considered a valid semantic. 
		/// </summary>
		/// <param name="session">The session within which the cascade is occurring. </param>
		/// <param name="child">The property value </param>
		/// <param name="parent">The property value owner </param>
		/// <param name="persister">The entity persister for the owner </param>
		/// <param name="propertyIndex">The index of the property within the owner. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public virtual Task NoCascadeAsync(IEventSource session, object child, object parent, IEntityPersister persister, int propertyIndex, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			try
			{
				NoCascade(session, child, parent, persister, propertyIndex);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		#endregion

		private partial class DeleteCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to delete: " + entityName);
					}
					return session.DeleteAsync(entityName, child, isCascadeDeleteEnabled, (ISet<object>)anything, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}

		private partial class LockCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to lock: " + entityName);
					}
					return session.LockAsync(entityName, child, LockMode.None, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}

		private partial class RefreshCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to refresh: " + entityName);
					}
					return session.RefreshAsync(child, (IDictionary)anything, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}

		private partial class EvictCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to evict: " + entityName);
					}
					return session.EvictAsync(child, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}

		private partial class SaveUpdateCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to saveOrUpdate: " + entityName);
					}
					return session.SaveOrUpdateAsync(entityName, child, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}

		private partial class MergeCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to merge: " + entityName);
					}
					return session.MergeAsync(entityName, child, (IDictionary)anything, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}
        
		private partial class PersistCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to persist: " + entityName);
					}
					return session.PersistAsync(entityName, child, (IDictionary)anything, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}

		private partial class PersistOnFlushCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to persistOnFlush: " + entityName);
					}
					return session.PersistOnFlushAsync(entityName, child, (IDictionary)anything, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}

			public override async Task NoCascadeAsync(IEventSource session, object child, object parent, IEntityPersister persister, int propertyIndex, CancellationToken cancellationToken)
			{
				cancellationToken.ThrowIfCancellationRequested();
				if (child == null)
				{
					return;
				}
				IType type = persister.PropertyTypes[propertyIndex];
				if (type.IsEntityType)
				{
					string childEntityName = ((EntityType)type).GetAssociatedEntityName(session.Factory);

					if (!IsInManagedState(child, session.PersistenceContext) && !child.IsProxy() && await (ForeignKeys.IsTransientSlowAsync(childEntityName, child, session, cancellationToken)).ConfigureAwait(false))
					{
						string parentEntiytName = persister.EntityName;
						string propertyName = persister.PropertyNames[propertyIndex];
						throw new TransientObjectException(
							string.Format(
								"object references an unsaved transient instance - save the transient instance before flushing or set cascade action for the property to something that would make it autosave: {0}.{1} -> {2}",
								parentEntiytName, propertyName, childEntityName));
					}
				}
			}
		}

		private partial class ReplicateCascadingAction : CascadingAction
		{
			public override Task CascadeAsync(IEventSource session, object child, string entityName, object anything, bool isCascadeDeleteEnabled, CancellationToken cancellationToken)
			{
				if (cancellationToken.IsCancellationRequested)
				{
					return Task.FromCanceled<object>(cancellationToken);
				}
				try
				{
					if (log.IsDebugEnabled)
					{
						log.Debug("cascading to replicate: " + entityName);
					}
					return session.ReplicateAsync(entityName, child, (ReplicationMode)anything, cancellationToken);
				}
				catch (Exception ex)
				{
					return Task.FromException<object>(ex);
				}
			}
		}
	}
}
