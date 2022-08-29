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
using NHibernate.Engine;
using NHibernate.Intercept;
using NHibernate.Persister.Collection;
using NHibernate.Properties;
using NHibernate.Tuple;

namespace NHibernate.Type
{
	using System.Threading.Tasks;
	using System.Threading;
	public static partial class TypeHelper
	{

		/// <summary>Apply the <see cref="ICacheAssembler.BeforeAssembleAsync(object,ISessionImplementor,CancellationToken)" /> operation across a series of values.</summary>
		/// <param name="row">The values</param>
		/// <param name="types">The value types</param>
		/// <param name="session">The originating session</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		public static async Task BeforeAssembleAsync(object[] row, ICacheAssembler[] types, ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			for (int i = 0; i < types.Length; i++)
			{
				if (!Equals(LazyPropertyInitializer.UnfetchedProperty, row[i]) && !Equals(BackrefPropertyAccessor.Unknown, row[i]))
				{
					await (types[i].BeforeAssembleAsync(row[i], session, cancellationToken)).ConfigureAwait(false);
				}
			}
		}

		/// <summary>
		/// Apply the <see cref="ICacheAssembler.AssembleAsync(object,ISessionImplementor,object,CancellationToken)" /> operation across a series of values.
		/// </summary>
		/// <param name="row">The values</param>
		/// <param name="types">The value types</param>
		/// <param name="session">The originating session</param>
		/// <param name="owner">The entity "owning" the values</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns></returns>
		public static async Task<object[]> AssembleAsync(object[] row, ICacheAssembler[] types, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var assembled = new object[row.Length];
			for (int i = 0; i < row.Length; i++)
			{
				if (Equals(LazyPropertyInitializer.UnfetchedProperty, row[i]) || Equals(BackrefPropertyAccessor.Unknown, row[i]))
				{
					assembled[i] = row[i];
				}
				else
				{
					assembled[i] = await (types[i].AssembleAsync(row[i], session, owner, cancellationToken)).ConfigureAwait(false);
				}
			}
			return assembled;
		}

		/// <summary>
		/// Apply the <see cref="ICacheAssembler.AssembleAsync(object,ISessionImplementor,object,CancellationToken)" /> operation across a series of values.
		/// </summary>
		/// <param name="row">The cached values.</param>
		/// <param name="types">The value types.</param>
		/// <param name="typeIndexes">The indexes of types to assemble.</param>
		/// <param name="session">The originating session.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>A new array of assembled values.</returns>
		internal static async Task<object[]> AssembleAsync(
			object[] row,
			ICacheAssembler[] types,
			IList<int> typeIndexes,
			ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var assembled = new object[row.Length];
			foreach (var i in typeIndexes)
			{
				var value = row[i];
				if (Equals(LazyPropertyInitializer.UnfetchedProperty, value) || Equals(BackrefPropertyAccessor.Unknown, value))
				{
					assembled[i] = value;
				}
				else
				{
					assembled[i] = await (types[i].AssembleAsync(row[i], session, null, cancellationToken)).ConfigureAwait(false);
				}
			}

			return assembled;
		}

		/// <summary>
		/// Initialize collections from the query cached row and update the assembled row.
		/// </summary>
		/// <param name="cacheRow">The cached values.</param>
		/// <param name="assembleRow">The assembled values to update.</param>
		/// <param name="collectionIndexes">The dictionary containing collection persisters and their indexes in the <paramref name="cacheRow"/> parameter as key.</param>
		/// <param name="session">The originating session.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		internal static async Task InitializeCollectionsAsync(
			object[] cacheRow,
			object[] assembleRow,
			IDictionary<int, ICollectionPersister> collectionIndexes,
			ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			foreach (var pair in collectionIndexes)
			{
				var value = cacheRow[pair.Key];
				if (value == null)
				{
					continue;
				}

				var collection = session.PersistenceContext.GetCollection(new CollectionKey(pair.Value, value));
				await (collection.ForceInitializationAsync(cancellationToken)).ConfigureAwait(false);
				assembleRow[pair.Key] = collection;
			}
		}

		/// <summary>Apply the <see cref="ICacheAssembler.DisassembleAsync(object,ISessionImplementor,object,CancellationToken)" /> operation across a series of values.</summary>
		/// <param name="row">The values</param>
		/// <param name="types">The value types</param>
		/// <param name="nonCacheable">An array indicating which values to include in the disassembled state</param>
		/// <param name="session">The originating session</param>
		/// <param name="owner">The entity "owning" the values</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns> The disassembled state</returns>
		public static async Task<object[]> DisassembleAsync(object[] row, ICacheAssembler[] types, bool[] nonCacheable, ISessionImplementor session, object owner, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] disassembled = new object[row.Length];
			for (int i = 0; i < row.Length; i++)
			{
				if (nonCacheable != null && nonCacheable[i])
				{
					disassembled[i] = LazyPropertyInitializer.UnfetchedProperty;
				}
				else if (Equals(LazyPropertyInitializer.UnfetchedProperty, row[i]) || Equals(BackrefPropertyAccessor.Unknown, row[i]))
				{
					disassembled[i] = row[i];
				}
				else
				{
					if (owner == null && row[i] is IPersistentCollection collection)
					{
						disassembled[i] = await (types[i].DisassembleAsync(row[i], session, collection.Owner, cancellationToken)).ConfigureAwait(false);
					}
					else
					{
						disassembled[i] = await (types[i].DisassembleAsync(row[i], session, owner, cancellationToken)).ConfigureAwait(false);
					}
				}
			}
			return disassembled;
		}

		/// <summary>
		/// Apply the <see cref="IType.ReplaceAsync(object, object, ISessionImplementor, object, IDictionary,CancellationToken)" /> operation across a series of values.
		/// </summary>
		/// <param name="original">The source of the state</param>
		/// <param name="target">The target into which to replace the source values.</param>
		/// <param name="types">The value types</param>
		/// <param name="session">The originating session</param>
		/// <param name="owner">The entity "owning" the values</param>
		/// <param name="copiedAlready">Represent a cache of already replaced state</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns> The replaced state</returns>
		public static async Task<object[]> ReplaceAsync(object[] original, object[] target, IType[] types, ISessionImplementor session,
																	 object owner, IDictionary copiedAlready, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			var copied = new object[original.Length];
			for (int i = 0; i < original.Length; i++)
			{
				if (Equals(LazyPropertyInitializer.UnfetchedProperty, original[i]) || Equals(BackrefPropertyAccessor.Unknown, original[i]))
				{
					copied[i] = target[i];
				}
				else if (target[i] == LazyPropertyInitializer.UnfetchedProperty)
				{
					// Should be no need to check for target[i] == PropertyAccessStrategyBackRefImpl.UNKNOWN
					// because PropertyAccessStrategyBackRefImpl.get( object ) returns
					// PropertyAccessStrategyBackRefImpl.UNKNOWN, so target[i] == original[i].
					//
					// We know from above that original[i] != LazyPropertyInitializer.UNFETCHED_PROPERTY &&
					// original[i] != PropertyAccessStrategyBackRefImpl.UNKNOWN;
					// This is a case where the entity being merged has a lazy property
					// that has been initialized. Copy the initialized value from original.
					if (types[i].IsMutable)
					{
						copied[i] = types[i].DeepCopy(original[i], session.Factory);
					}
					else
					{
						copied[i] = original[i];
					}
				}
				else
				{
					copied[i] = await (types[i].ReplaceAsync(original[i], target[i], session, owner, copiedAlready, cancellationToken)).ConfigureAwait(false);
				}
			}
			return copied;
		}

		/// <summary>
		/// Apply the <see cref="IType.ReplaceAsync(object, object, ISessionImplementor, object, IDictionary, ForeignKeyDirection,CancellationToken)" />
		/// operation across a series of values.
		/// </summary>
		/// <param name="original">The source of the state</param>
		/// <param name="target">The target into which to replace the source values.</param>
		/// <param name="types">The value types</param>
		/// <param name="session">The originating session</param>
		/// <param name="owner">The entity "owning" the values</param>
		/// <param name="copyCache">A map representing a cache of already replaced state</param>
		/// <param name="foreignKeyDirection">FK directionality to be applied to the replacement</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns> The replaced state</returns>
		public static async Task<object[]> ReplaceAsync(object[] original, object[] target, IType[] types,
			ISessionImplementor session, object owner, IDictionary copyCache, ForeignKeyDirection foreignKeyDirection, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] copied = new object[original.Length];
			for (int i = 0; i < types.Length; i++)
			{
				if (Equals(LazyPropertyInitializer.UnfetchedProperty, original[i]) || Equals(BackrefPropertyAccessor.Unknown, original[i]))
				{
					copied[i] = target[i];
				}
				else if (target[i] == LazyPropertyInitializer.UnfetchedProperty)
				{
					// Should be no need to check for target[i] == PropertyAccessStrategyBackRefImpl.UNKNOWN
					// because PropertyAccessStrategyBackRefImpl.get( object ) returns
					// PropertyAccessStrategyBackRefImpl.UNKNOWN, so target[i] == original[i].
					//
					// We know from above that original[i] != LazyPropertyInitializer.UNFETCHED_PROPERTY &&
					// original[i] != PropertyAccessStrategyBackRefImpl.UNKNOWN;
					// This is a case where the entity being merged has a lazy property
					// that has been initialized. Copy the initialized value from original.
					if (types[i].IsMutable)
					{
						copied[i] = types[i].DeepCopy(original[i], session.Factory);
					}
					else
					{
						copied[i] = original[i];
					}
				}
				else
					copied[i] = await (types[i].ReplaceAsync(original[i], target[i], session, owner, copyCache, foreignKeyDirection, cancellationToken)).ConfigureAwait(false);
			}
			return copied;
		}

		/// <summary>
		/// Apply the <see cref="IType.ReplaceAsync(object, object, ISessionImplementor, object, IDictionary, ForeignKeyDirection,CancellationToken)" />
		/// operation across a series of values, as long as the corresponding <see cref="IType"/> is an association.
		/// </summary>
		/// <param name="original">The source of the state</param>
		/// <param name="target">The target into which to replace the source values.</param>
		/// <param name="types">The value types</param>
		/// <param name="session">The originating session</param>
		/// <param name="owner">The entity "owning" the values</param>
		/// <param name="copyCache">A map representing a cache of already replaced state</param>
		/// <param name="foreignKeyDirection">FK directionality to be applied to the replacement</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns> The replaced state</returns>
		/// <remarks>
		/// If the corresponding type is a component type, then apply <see cref="ReplaceAssociationsAsync(object[],object[],IType[],ISessionImplementor,object,IDictionary,ForeignKeyDirection,CancellationToken)" />
		/// across the component subtypes but do not replace the component value itself.
		/// </remarks>
		public static async Task<object[]> ReplaceAssociationsAsync(object[] original, object[] target, IType[] types,
			ISessionImplementor session, object owner, IDictionary copyCache, ForeignKeyDirection foreignKeyDirection, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			object[] copied = new object[original.Length];
			for (int i = 0; i < types.Length; i++)
			{
				if (Equals(LazyPropertyInitializer.UnfetchedProperty, original[i]) || Equals(BackrefPropertyAccessor.Unknown, original[i]))
				{
					copied[i] = target[i];
				}
				else if (types[i].IsComponentType)
				{
					// need to extract the component values and check for subtype replacements...
					IAbstractComponentType componentType = (IAbstractComponentType) types[i];
					IType[] subtypes = componentType.Subtypes;
					object[] origComponentValues = original[i] == null ? new object[subtypes.Length] : await (componentType.GetPropertyValuesAsync(original[i], session, cancellationToken)).ConfigureAwait(false);
					object[] targetComponentValues = target[i] == null ? new object[subtypes.Length] : await (componentType.GetPropertyValuesAsync(target[i], session, cancellationToken)).ConfigureAwait(false);

					object[] componentCopy = await (ReplaceAssociationsAsync(origComponentValues, targetComponentValues, subtypes, session, null, copyCache, foreignKeyDirection, cancellationToken)).ConfigureAwait(false);

					if (!componentType.IsAnyType && target[i] != null)
						componentType.SetPropertyValues(target[i], componentCopy);

					copied[i] = target[i];
				}
				else if (!types[i].IsAssociationType)
				{
					copied[i] = target[i];
				}
				else
				{
					copied[i] = await (types[i].ReplaceAsync(original[i], target[i], session, owner, copyCache, foreignKeyDirection, cancellationToken)).ConfigureAwait(false);
				}
			}
			return copied;
		}

		/// <summary>
		/// <para>Determine if any of the given field values are dirty, returning an array containing
		/// indices of the dirty fields.</para>
		/// <para>If it is determined that no fields are dirty, null is returned.</para>
		/// </summary>
		/// <param name="properties">The property definitions</param>
		/// <param name="currentState">The current state of the entity</param>
		/// <param name="previousState">The baseline state of the entity</param>
		/// <param name="includeColumns">Columns to be included in the dirty checking, per property</param>
		/// <param name="anyUninitializedProperties">Does the entity currently hold any uninitialized property values?</param>
		/// <param name="session">The session from which the dirty check request originated.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>Array containing indices of the dirty properties, or null if no properties considered dirty.</returns>
		// Since 5.3
		[Obsolete("Use overload without anyUninitializedProperties parameter instead")]
		public static Task<int[]> FindDirtyAsync(StandardProperty[] properties,
										object[] currentState,
										object[] previousState,
										bool[][] includeColumns,
										bool anyUninitializedProperties,
										ISessionImplementor session, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int[]>(cancellationToken);
			}
			return FindDirtyAsync(properties, currentState, previousState, includeColumns, session, cancellationToken);
		}

		/// <summary>
		/// <para>Determine if any of the given field values are dirty, returning an array containing
		/// indices of the dirty fields.</para>
		/// <para>If it is determined that no fields are dirty, null is returned.</para>
		/// </summary>
		/// <param name="properties">The property definitions</param>
		/// <param name="currentState">The current state of the entity</param>
		/// <param name="previousState">The baseline state of the entity</param>
		/// <param name="includeColumns">Columns to be included in the dirty checking, per property</param>
		/// <param name="session">The session from which the dirty check request originated.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>Array containing indices of the dirty properties, or null if no properties considered dirty.</returns>
		public static async Task<int[]> FindDirtyAsync(StandardProperty[] properties,
										object[] currentState,
										object[] previousState,
										bool[][] includeColumns,
										ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int[] results = null;
			int count = 0;
			int span = properties.Length;

			for (int i = 0; i < span; i++)
			{
				var dirty = await (DirtyAsync(properties, currentState, previousState, includeColumns, session, i, cancellationToken)).ConfigureAwait(false);
				if (dirty)
				{
					if (results == null)
					{
						results = new int[span];
					}
					results[count++] = i;
				}
			}
			if (count == 0)
			{
				return null;
			}
			else
			{
				int[] trimmed = new int[count];
				Array.Copy(results, 0, trimmed, 0, count);
				return trimmed;
			}
		}

		private static async Task<bool> DirtyAsync(StandardProperty[] properties, object[] currentState, object[] previousState, bool[][] includeColumns, ISessionImplementor session, int i, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			if (Equals(LazyPropertyInitializer.UnfetchedProperty, currentState[i]))
				return false;
			if (Equals(LazyPropertyInitializer.UnfetchedProperty, previousState[i]))
				return true;
			return properties[i].IsDirtyCheckable() &&
				   await (properties[i].Type.IsDirtyAsync(previousState[i], currentState[i], includeColumns[i], session, cancellationToken)).ConfigureAwait(false);
		}

		/// <summary>
		/// <para>Determine if any of the given field values are modified, returning an array containing
		/// indices of the modified fields.</para>
		/// <para>If it is determined that no fields are dirty, null is returned.</para>
		/// </summary>
		/// <param name="properties">The property definitions</param>
		/// <param name="currentState">The current state of the entity</param>
		/// <param name="previousState">The baseline state of the entity</param>
		/// <param name="includeColumns">Columns to be included in the mod checking, per property</param>
		/// <param name="anyUninitializedProperties">Does the entity currently hold any uninitialized property values?</param>
		/// <param name="session">The session from which the dirty check request originated.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>Array containing indices of the modified properties, or null if no properties considered modified.</returns>
		// Since 5.3
		[Obsolete("Use the overload without anyUninitializedProperties parameter.")]
		public static Task<int[]> FindModifiedAsync(StandardProperty[] properties,
											object[] currentState,
											object[] previousState,
											bool[][] includeColumns,
											bool anyUninitializedProperties,
											ISessionImplementor session, CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				return Task.FromCanceled<int[]>(cancellationToken);
			}
			return FindModifiedAsync(properties, currentState, previousState, includeColumns, session, cancellationToken);
		}

		/// <summary>
		/// <para>Determine if any of the given field values are modified, returning an array containing
		/// indices of the modified fields.</para>
		/// <para>If it is determined that no fields are dirty, null is returned.</para>
		/// </summary>
		/// <param name="properties">The property definitions</param>
		/// <param name="currentState">The current state of the entity</param>
		/// <param name="previousState">The baseline state of the entity</param>
		/// <param name="includeColumns">Columns to be included in the mod checking, per property</param>
		/// <param name="session">The session from which the dirty check request originated.</param>
		/// <param name="cancellationToken">A cancellation token that can be used to cancel the work</param>
		/// <returns>Array containing indices of the modified properties, or null if no properties considered modified.</returns>
		public static async Task<int[]> FindModifiedAsync(StandardProperty[] properties,
											object[] currentState,
											object[] previousState,
											bool[][] includeColumns,
											ISessionImplementor session, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();
			int[] results = null;
			int count = 0;
			int span = properties.Length;

			for (int i = 0; i < span; i++)
			{
				bool dirty =
					!Equals(LazyPropertyInitializer.UnfetchedProperty, currentState[i]) &&
					properties[i].IsDirtyCheckable()
					&& await (properties[i].Type.IsModifiedAsync(previousState[i], currentState[i], includeColumns[i], session, cancellationToken)).ConfigureAwait(false);

				if (dirty)
				{
					if (results == null)
					{
						results = new int[span];
					}
					results[count++] = i;
				}
			}
			if (count == 0)
			{
				return null;
			}
			else
			{
				int[] trimmed = new int[count];
				Array.Copy(results, 0, trimmed, 0, count);
				return trimmed;
			}
		}
	}
}
