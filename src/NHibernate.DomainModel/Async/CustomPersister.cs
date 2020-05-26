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
using NHibernate.Cache;
using NHibernate.Cache.Entry;
using NHibernate.Engine;
using NHibernate.Event;
using NHibernate.Id;
using NHibernate.Mapping;
using NHibernate.Metadata;
using NHibernate.Persister.Entity;
using NHibernate.Tuple.Entity;
using NHibernate.Type;
using NHibernate.Util;
using Array = System.Array;

namespace NHibernate.DomainModel
{
	using System.Threading.Tasks;
	using System.Threading;
	public partial class CustomPersister : IEntityPersister
	{

		#region IEntityPersister Members

		#region IOptimisticCacheSource Members

		public Task<int[]> FindDirtyAsync(object[] currentState, object[] previousState, object entity, ISessionImplementor session, CancellationToken cancellationToken)
		{
			try
			{
				if (!Equals(currentState[0], previousState[0]))
				{
					return Task.FromResult<int[]>(new int[] { 0 });
				}
				else
				{
					return Task.FromResult<int[]>(null);
				}
			}
			catch (Exception ex)
			{
				return Task.FromException<int[]>(ex);
			}
		}

		public Task<int[]> FindModifiedAsync(object[] old, object[] current, object entity, ISessionImplementor session, CancellationToken cancellationToken)
		{
			try
			{
				if (!Equals(old[0], current[0]))
				{
					return Task.FromResult<int[]>(new int[] { 0 });
				}
				else
				{
					return Task.FromResult<int[]>(null);
				}
			}
			catch (Exception ex)
			{
				return Task.FromException<int[]>(ex);
			}
		}

		public Task<object[]> GetNaturalIdentifierSnapshotAsync(object id, ISessionImplementor session, CancellationToken cancellationToken)
		{
			return Task.FromResult<object[]>(null);
		}

		public async Task<object> LoadAsync(object id, object optionalObject, LockMode lockMode, ISessionImplementor session, CancellationToken cancellationToken)
		{
			// fails when optional object is supplied
			Custom clone = null;
			Custom obj = (Custom)Instances[id];
			if (obj != null)
			{
				clone = (Custom)obj.Clone();
				TwoPhaseLoad.AddUninitializedEntity(session.GenerateEntityKey(id, this), clone, this, LockMode.None, session);
				TwoPhaseLoad.PostHydrate(this, id, new String[] {obj.Name}, null, clone, LockMode.None, session);
				await (TwoPhaseLoad.InitializeEntityAsync(clone, false, session, new PreLoadEvent((IEventSource) session),
				                              new PostLoadEvent((IEventSource) session), cancellationToken));
			}
			return clone;
		}

		public Task LockAsync(object id, object version, object obj, LockMode lockMode, ISessionImplementor session, CancellationToken cancellationToken)
		{
			throw new NotSupportedException();
		}

		public Task InsertAsync(object id, object[] fields, object obj, ISessionImplementor session, CancellationToken cancellationToken)
		{
			try
			{
				Instances[id] = ((Custom)obj).Clone();
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task<object> InsertAsync(object[] fields, object obj, ISessionImplementor session, CancellationToken cancellationToken)
		{
			throw new NotSupportedException();
		}

		public Task DeleteAsync(object id, object version, object obj, ISessionImplementor session, CancellationToken cancellationToken)
		{
			try
			{
				Instances.Remove(id);
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task UpdateAsync(object id, object[] fields, int[] dirtyFields, bool hasDirtyCollection, object[] oldFields,
		                   object oldVersion, object obj, object rowId, ISessionImplementor session, CancellationToken cancellationToken)
		{
			try
			{
				Instances[id] = ((Custom)obj).Clone();
				return Task.CompletedTask;
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task<object[]> GetDatabaseSnapshotAsync(object id, ISessionImplementor session, CancellationToken cancellationToken)
		{
			return Task.FromResult<object[]>(null);
		}

		public Task<object> GetCurrentVersionAsync(object id, ISessionImplementor session, CancellationToken cancellationToken)
		{
			try
			{
				return Task.FromResult<object>(Instances[id]);
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}

		public Task<object> ForceVersionIncrementAsync(object id, object currentVersion, ISessionImplementor session, CancellationToken cancellationToken)
		{
			return Task.FromResult<object>(null);
		}

		public Task<bool?> IsTransientAsync(object obj, ISessionImplementor session)
		{
			try
			{
				return Task.FromResult<bool?>(((Custom) obj).Id == null);
			}
			catch (Exception ex)
			{
				return Task.FromException<bool?>(ex);
			}
		}

		public Task ProcessInsertGeneratedPropertiesAsync(object id, object entity, object[] state, ISessionImplementor session, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		public Task ProcessUpdateGeneratedPropertiesAsync(object id, object entity, object[] state, ISessionImplementor session, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		#endregion

		#endregion
	}
}
