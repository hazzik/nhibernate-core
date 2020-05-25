using System;
using NHibernate.Cfg.MappingSchema;

namespace NHibernate.Cfg
{
	public class BindMappingEventArgs : EventArgs
	{
		//6.0 TODO: Remove

		public BindMappingEventArgs(HbmMapping mapping, string fileName)
		{
			Mapping = mapping;
			FileName = fileName;
		}

		public HbmMapping Mapping { get; }
		public string FileName { get; }
	}
}
