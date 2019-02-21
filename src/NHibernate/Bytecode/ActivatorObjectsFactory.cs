using System;

namespace NHibernate.Bytecode
{
	public class ActivatorObjectsFactory: IObjectsFactory
	{
		public object CreateInstance(System.Type type)
		{
			return Activator.CreateInstance(type);
		}
	}
}
