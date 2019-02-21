using NHibernate.Properties;

namespace NHibernate.Bytecode.Lightweight
{
	/// <summary>
	/// Factory that generate object based on IReflectionOptimizer needed to replace the use
	/// of reflection.
	/// </summary>
	/// <remarks>
	/// Used in <see cref="NHibernate.Persister.Entity.AbstractEntityPersister"/> and
	/// <see cref="NHibernate.Type.ComponentType"/>
	/// </remarks>
	public class BytecodeProviderImpl : AbstractBytecodeProvider
	{
		/// <inheritdoc/>
		public override IReflectionOptimizer GetReflectionOptimizer(System.Type mappedClass, IGetter[] getters, ISetter[] setters, IGetter specializedGetter, ISetter specializedSetter)
		{
			return new ReflectionOptimizer(mappedClass, getters, setters, specializedGetter, specializedSetter);
		}
	}
}
