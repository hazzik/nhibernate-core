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
using NHibernate.Action;
using NHibernate.Classic;
using NHibernate.Engine;
using NHibernate.Impl;
using NHibernate.Persister.Entity;
using NHibernate.Type;
using NHibernate.Util;
using Status=NHibernate.Engine.Status;

namespace NHibernate.Event.Default
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class DefaultDeleteEventListener : IDeleteEventListener
	{

		#region IDeleteEventListener Members

		/// <summary>Handle the given delete event. </summary>
		/// <param name="event">The delete event to be handled. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public virtual Task OnDeleteAsync(DeleteEvent @event, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<object>(cancellationToken);
			}
			return OnDeleteAsync(@event, new IdentitySet(), cancellationToken);
		}

		public virtual async Task OnDeleteAsync(DeleteEvent @event, ISet<object> transientEntities, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			IEventSource source = @event.Session;
			IPersistenceContext persistenceContext = source.PersistenceContext;
			object entity = await (persistenceContext.UnproxyAndReassociateAsync(@event.Entity, cancellationToken)).ConfigureAwait(false);

			EntityEntry entityEntry = persistenceContext.GetEntry(entity);
			IEntityPersister persister;
			object id;
			object version;

			if (entityEntry == null)
			{
				log.Debug("entity was not persistent in delete processing");

				persister = source.GetEntityPersister(@event.EntityName, entity);

				if (await (ForeignKeys.IsTransientSlowAsync(persister.EntityName, entity, source, cancellationToken)).ConfigureAwait(false))
				{
					await (DeleteTransientEntityAsync(source, entity, @event.CascadeDeleteEnabled, persister, transientEntities, cancellationToken)).ConfigureAwait(false);
					// EARLY EXIT!!!
					return;
				}
				else
				{
					PerformDetachedEntityDeletionCheck(@event);
				}

				id = persister.GetIdentifier(entity);

				if (id == null)
				{
					throw new TransientObjectException("the detached instance passed to delete() had a null identifier");
				}

				EntityKey key = source.GenerateEntityKey(id, persister);

				persistenceContext.CheckUniqueness(key, entity);

				await (new OnUpdateVisitor(source, id, entity).ProcessAsync(entity, persister, cancellationToken)).ConfigureAwait(false);

				version = persister.GetVersion(entity);

				entityEntry = persistenceContext.AddEntity(
					entity, 
					persister.IsMutable ? Status.Loaded : Status.ReadOnly,
					persister.GetPropertyValues(entity), 
					key,
					version, 
					LockMode.None, 
					true, 
					persister, 
					false, 
					false);
			}
			else
			{
				log.Debug("deleting a persistent instance");

				if (entityEntry.Status == Status.Deleted || entityEntry.Status == Status.Gone)
				{
					log.Debug("object was already deleted");
					return;
				}
				persister = entityEntry.Persister;
				id = entityEntry.Id;
				version = entityEntry.Version;
			}

			if (InvokeDeleteLifecycle(source, entity, persister))
				return;

			await (DeleteEntityAsync(source, entity, entityEntry, @event.CascadeDeleteEnabled, persister, transientEntities, cancellationToken)).ConfigureAwait(false);

			if (source.Factory.Settings.IsIdentifierRollbackEnabled)
			{
				persister.ResetIdentifier(entity, id, version);
			}
		}

		#endregion

		/// <summary> 
		/// We encountered a delete request on a transient instance.
		/// <p/>
		/// This is a deviation from historical Hibernate (pre-3.2) behavior to
		/// align with the JPA spec, which states that transient entities can be
		/// passed to remove operation in which case cascades still need to be
		/// performed.
		///  </summary>
		/// <param name="session">The session which is the source of the event </param>
		/// <param name="entity">The entity being delete processed </param>
		/// <param name="cascadeDeleteEnabled">Is cascading of deletes enabled</param>
		/// <param name="persister">The entity persister </param>
		/// <param name="transientEntities">
		/// A cache of already visited transient entities (to avoid infinite recursion).
		/// </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		protected virtual async Task DeleteTransientEntityAsync(IEventSource session, object entity, bool cascadeDeleteEnabled, IEntityPersister persister, ISet<object> transientEntities, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			log.Info("handling transient entity in delete processing");
			// NH different impl : NH-1895
			if(transientEntities == null)
			{
				transientEntities = new HashSet<object>();
			}
			if (!transientEntities.Add(entity))
			{
				log.Debug("already handled transient entity; skipping");
				return;
			}
			await (CascadeBeforeDeleteAsync(session, persister, entity, null, transientEntities, cancellationToken)).ConfigureAwait(false);
			await (CascadeAfterDeleteAsync(session, persister, entity, transientEntities, cancellationToken)).ConfigureAwait(false);
		}

		/// <summary> 
		/// Perform the entity deletion.  Well, as with most operations, does not
		/// really perform it; just schedules an action/execution with the
		/// <see cref="ActionQueue"/> for execution during flush. 
		/// </summary>
		/// <param name="session">The originating session </param>
		/// <param name="entity">The entity to delete </param>
		/// <param name="entityEntry">The entity's entry in the <see cref="ISession"/> </param>
		/// <param name="isCascadeDeleteEnabled">Is delete cascading enabled? </param>
		/// <param name="persister">The entity persister. </param>
		/// <param name="transientEntities">A cache of already deleted entities. </param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		protected virtual async Task DeleteEntityAsync(IEventSource session, object entity, EntityEntry entityEntry, bool isCascadeDeleteEnabled, IEntityPersister persister, ISet<object> transientEntities, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var factory = session.Factory;
			if (log.IsDebugEnabled)
			{
				log.Debug("deleting " + MessageHelper.InfoString(persister, entityEntry.Id, factory));
			}

			IPersistenceContext persistenceContext = session.PersistenceContext;

			IType[] propTypes = persister.PropertyTypes;
			object version = entityEntry.Version;

			object[] currentState;
			if (entityEntry.LoadedState == null)
			{
				//ie. the entity came in from update()
				currentState = persister.GetPropertyValues(entity);
			}
			else
			{
				currentState = entityEntry.LoadedState;
			}

			object[] deletedState = CreateDeletedState(persister, currentState, factory);
			entityEntry.DeletedState = deletedState;

			session.Interceptor.OnDelete(entity, entityEntry.Id, deletedState, persister.PropertyNames, propTypes);

			// before any callbacks, etc, so subdeletions see that this deletion happened first
			persistenceContext.SetEntryStatus(entityEntry, Status.Deleted);
			EntityKey key = session.GenerateEntityKey(entityEntry.Id, persister);

			await (CascadeBeforeDeleteAsync(session, persister, entity, entityEntry, transientEntities, cancellationToken)).ConfigureAwait(false);

			await (new ForeignKeys.Nullifier(entity, true, false, session).NullifyTransientReferencesAsync(entityEntry.DeletedState, propTypes, cancellationToken)).ConfigureAwait(false);
			new Nullability(session).CheckNullability(entityEntry.DeletedState, persister, true);
			persistenceContext.NullifiableEntityKeys.Add(key);

			// Ensures that containing deletions happen before sub-deletions
			session.ActionQueue.AddAction(new EntityDeleteAction(entityEntry.Id, deletedState, version, entity, persister, isCascadeDeleteEnabled, session));

			await (CascadeAfterDeleteAsync(session, persister, entity, transientEntities, cancellationToken)).ConfigureAwait(false);

			// the entry will be removed after the flush, and will no longer
			// override the stale snapshot
			// This is now handled by removeEntity() in EntityDeleteAction
			//persistenceContext.removeDatabaseSnapshot(key);
		}

		protected virtual async Task CascadeBeforeDeleteAsync(IEventSource session, IEntityPersister persister, object entity, EntityEntry entityEntry, ISet<object> transientEntities, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ISessionImplementor si = session;
			CacheMode cacheMode = si.CacheMode;
			si.CacheMode = CacheMode.Get;
			session.PersistenceContext.IncrementCascadeLevel();
			try
			{
				// cascade-delete to collections BEFORE the collection owner is deleted
				await (new Cascade(CascadingAction.Delete, CascadePoint.AfterInsertBeforeDelete, session).CascadeOnAsync(persister, entity,
				                                                                                             transientEntities, cancellationToken)).ConfigureAwait(false);
			}
			finally
			{
				session.PersistenceContext.DecrementCascadeLevel();
				si.CacheMode = cacheMode;
			}
		}

		protected virtual async Task CascadeAfterDeleteAsync(IEventSource session, IEntityPersister persister, object entity, ISet<object> transientEntities, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			ISessionImplementor si = session;
			CacheMode cacheMode = si.CacheMode;
			si.CacheMode = CacheMode.Get;
			session.PersistenceContext.IncrementCascadeLevel();
			try
			{
				// cascade-delete to many-to-one AFTER the parent was deleted
				await (new Cascade(CascadingAction.Delete, CascadePoint.BeforeInsertAfterDelete, session).CascadeOnAsync(persister, entity,
				                                                                                             transientEntities, cancellationToken)).ConfigureAwait(false);
			}
			finally
			{
				session.PersistenceContext.DecrementCascadeLevel();
				si.CacheMode = cacheMode;
			}
		}
	}
}
