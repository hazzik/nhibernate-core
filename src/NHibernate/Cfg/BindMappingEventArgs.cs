using System;
using NHibernate.Cfg.MappingSchema;

namespace NHibernate.Cfg
{
	public class BindMappingEventArgs : EventArgs
	{
		public BindMappingEventArgs(HbmMapping mapping, string fileName)
		{
			Mapping = mapping;
			FileName = fileName;
		}

		public HbmMapping Mapping { get; }
		public string FileName { get; }
	}
}
