using System.Reflection;

namespace NHibernate.Properties
{
	/// <summary>
	/// An <see cref="IGetter" /> or <see cref="ISetter"/> that can the member info.
	/// </summary>
	public interface IMemberInfoAccessor
	{
		MemberInfo MemberInfo { get; }
	}
}