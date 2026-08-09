using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1109
{
	class Device
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<DeviceAttribValue> SpecialAttributes { get; set; } = new List<DeviceAttribValue>();
	}

	class DeviceAttribValue
	{
		public virtual int Id { get; set; }
		public virtual string AttribValue { get; set; }
		public virtual Device Device { get; set; }
	}
}
