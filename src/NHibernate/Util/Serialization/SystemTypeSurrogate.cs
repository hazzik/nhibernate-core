using System.Runtime.Serialization;

namespace NHibernate.Util.Serialization
{
#if !NETFX
	public class SystemTypeSurrogate : ISerializationSurrogate
	{
		public void GetObjectData(object obj, SerializationInfo info, StreamingContext context)
		{
			var type = (System.Type) obj;
			info.AddValue("name", type.AssemblyQualifiedName);
		}

		public object SetObjectData(
			object obj,
			SerializationInfo info,
			StreamingContext context,
			ISurrogateSelector selector)
		{
			var name = info.GetString("name");
			return System.Type.GetType(name, true);
		}
	}
#endif
}