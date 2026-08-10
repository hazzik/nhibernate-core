using System;
using System.Data;
using System.Data.Common;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;

namespace NHibernate.Test.NHSpecificTest.GH1001
{
	// Mimics the custom IUserType from the original NH-2629 report: an ANSI string
	// that is right-trimmed on read.
	public class AnsiStringTrimmedType : IUserType
	{
		public SqlType[] SqlTypes => new[] {new SqlType(DbType.AnsiString)};

		public System.Type ReturnedType => typeof(string);

		public new bool Equals(object x, object y)
		{
			return Equals((string) x, (string) y);
		}

		public int GetHashCode(object x)
		{
			return x?.GetHashCode() ?? 0;
		}

		public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
		{
			var value = rs[names[0]];
			return value == DBNull.Value ? null : ((string) value).TrimEnd();
		}

		public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
		{
			NHibernateUtil.AnsiString.NullSafeSet(cmd, value, index, session);
		}

		public object DeepCopy(object value)
		{
			return value;
		}

		public bool IsMutable => false;

		public object Replace(object original, object target, object owner)
		{
			return original;
		}

		public object Assemble(object cached, object owner)
		{
			return cached;
		}

		public object Disassemble(object value)
		{
			return value;
		}
	}
}
