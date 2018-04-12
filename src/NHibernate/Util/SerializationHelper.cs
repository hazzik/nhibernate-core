using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace NHibernate.Util
{
	public static class SerializationHelper
	{
		private static readonly Lazy<BinaryFormatter> Formatter = new Lazy<BinaryFormatter>(CreateBinaryFormatter);

		public static byte[] Serialize(object obj)
		{
			using (var ms = new MemoryStream())
			{
				Formatter.Value.Serialize(ms, obj);
				return ms.ToArray();
			}
		}

		public static object Deserialize(byte[] data)
		{
			using (var ms = new MemoryStream(data))
			{
				return Formatter.Value.Deserialize(ms);
			}
		}

		private static BinaryFormatter CreateBinaryFormatter()
		{
			var formatter = new BinaryFormatter();
#if !NETFX
			var selector = new SurrogateSelector();

			selector.AddSurrogate(
				typeof(System.Type),
				new StreamingContext(StreamingContextStates.All),
				new Serialization.SystemTypeSurrogate());
			
			formatter.SurrogateSelector = selector;
#endif
			return formatter;
		}
	}
}