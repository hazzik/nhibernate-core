using System;
using System.Collections.Generic;
using System.Text;

namespace NHibernate.Bytecode
{
    /// <summary>
    /// Represents optimized entity specific property access.
    /// </summary>
    public interface IPropertyAccessOptimizer
    {
        /// <summary>
        /// Get value from specific property of entity.
        /// </summary>
        /// <param name="target">target where data need be taken</param>
        /// <returns>value of property of entity.</returns>
        object GetValue(object target);

        /// <summary>
        /// Set <paramref name="value"/> to specific property of <paramref name="target"/> entity.
        /// </summary>
        /// <param name="target">target which need be updated</param>
        /// <param name="value">value that need be set to specific property of entity</param>
        void SetValue(object target, object value);
    }
}
