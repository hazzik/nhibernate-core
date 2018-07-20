using System;
using System.Collections;

namespace NHibernate.Bytecode
{
	/// <summary>
	/// Represents reflection optimization for a particular class.
	/// </summary>
	public interface IReflectionOptimizer
	{
        /// <summary>
        /// Get optimizer for get/set valus to all properties
        /// </summary>
		IAccessOptimizer AccessOptimizer { get; }

        /// <summary>
        /// Get optimizer for get/set value to entity identifier.
        /// </summary>
        IPropertyAccessOptimizer IdentifierAccessOptimizer { get;}

        /// <summary>
        /// Get optimizer to create entity instantiation.<para/>
        /// </summary>
        IInstantiationOptimizer InstantiationOptimizer { get; }
	}
}