using NHibernate.Properties;

namespace NHibernate.Bytecode
{
	/// <summary>
	/// A <see cref="IBytecodeProvider" /> implementation that returns
	/// <see langword="null" />, disabling reflection optimization.
	/// </summary>
	public class NullBytecodeProvider : AbstractBytecodeProvider
	{
		public override IReflectionOptimizer GetReflectionOptimizer(System.Type clazz, IGetter[] getters, ISetter[] setters, IGetter specializedGetter, ISetter specializedSetter)
		{
			return null;
		}
	}
}
