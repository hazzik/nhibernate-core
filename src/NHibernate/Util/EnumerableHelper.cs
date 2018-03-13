using System;
using System.Collections;

namespace NHibernate.Util
{
	internal static class EnumerableHelper
	{
		// Do not convert to an extension method, otherwise it may take precedence over the .Net standard extension
		// in some cases.
		internal static int Count(IEnumerable source)
		{
			if (source is ICollection col)
				return col.Count;

			var count = 0;
			var e = source.GetEnumerator();
			//in case it's generic disposable enumerator
			using (e as IDisposable)
			{
				while (e.MoveNext())
				{
					count++;
				}
			}
			return count;
		}
	}
}
