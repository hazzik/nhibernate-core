using System;

namespace NHibernate.Bytecode
{
	public class ActivatorObjectsFactory: IObjectsFactory
	{
		public object CreateInstance(System.Type type)
		{
			return Activator.CreateInstance(type);
		}

		// Since v5.2

		// Since v5.2
	}
}
