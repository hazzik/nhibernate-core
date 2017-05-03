using System;

namespace NHibernate.Type
{
	/// <summary>
	/// An <see cref="IType"/> that maps a sorted <c>ISet{T}</c> collection
	/// to the database.
	/// </summary>
	[Serializable]
	public class GenericOrderedSetType<T> : GenericSetType<T>
	{
		/// <summary>
		/// Initializes a new instance of a <see cref="GenericOrderedSetType{T}"/> class for
		/// a specific role.
		/// </summary>
		/// <param name="role">The role the persistent collection is in.</param>
		/// <param name="propertyRef">The name of the property in the
		/// owner object containing the collection ID, or <see langword="null" /> if it is
		/// the primary key.</param>
		public GenericOrderedSetType(string role, string propertyRef)
			: base(role, propertyRef)
		{
		}

		public override object Instantiate(int anticipatedSize)
		{
#if !NETCOREAPP2_0
			return new Iesi.Collections.Generic.LinkedHashSet<T>();
#else
			throw new NotSupportedException();
#endif
		}
	}
}
