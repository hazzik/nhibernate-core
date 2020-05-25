namespace NHibernate.Bytecode
{
	/// <summary>
	/// Interface for instantiating NHibernate dependencies.
	/// </summary>
	public interface IObjectsFactory
	{
		/// <summary>
		/// Creates an instance of the specified type.
		/// </summary>
		/// <param name="type">The type of object to create.</param>
		/// <returns>A reference to the created object.</returns>
		object CreateInstance(System.Type type);
	}
}
