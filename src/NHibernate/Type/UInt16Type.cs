using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Numerics;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// Maps a <see cref="System.UInt16"/> Property 
	/// to a <see cref="DbType.UInt16"/> column.
	/// </summary>
	[Serializable]
	public partial class UInt16Type : PrimitiveType, IDiscriminatorType, IVersionType
	{
		private static readonly object ZeroObject = (ushort) 0;

		/// <summary />
		public UInt16Type() : base(SqlTypeFactory.UInt16)
		{
		}

		/// <summary></summary>
		public override string Name => "UInt16";

		public override object Get(DbDataReader rs, int index, ISessionImplementor session)
		{
			try
			{
				return rs[index] switch
				{
					BigInteger bi => (ushort) bi,
					var c => Convert.ToUInt16(c)
				};
			}
			catch (Exception ex)
			{
				throw new FormatException(string.Format("Input string '{0}' was not in the correct format.", rs[index]), ex);
			}
		}

		public override System.Type ReturnedClass => typeof(UInt16);

		public override void Set(DbCommand rs, object value, int index, ISessionImplementor session)
		{
			rs.Parameters[index].Value = Convert.ToUInt16(value);
		}

		// 6.0 TODO: rename "xml" parameter as "value": it is not a xml string. The fact it generally comes from a xml
		// attribute value is irrelevant to the method behavior.
		/// <inheritdoc />
		public object StringToObject(string xml)
		{
			// 6.0 TODO: remove warning disable/restore
#pragma warning disable 618
			return FromStringValue(xml);
#pragma warning restore 618
		}

		// 6.0 TODO: rename "xml" parameter as "value": it is not a xml string. The fact it generally comes from a xml
		// attribute value is irrelevant to the method behavior. Replace override keyword by virtual after having
		// removed the obsoleted base.
		/// <inheritdoc cref="IVersionType.FromStringValue"/>
#pragma warning disable 672
		public override object FromStringValue(string xml)
#pragma warning restore 672
		{
			return UInt16.Parse(xml);
		}

		#region IVersionType Members

		public virtual object Next(object current, ISessionImplementor session)
		{
			return (UInt16)current + 1;
		}

		public virtual object Seed(ISessionImplementor session)
		{
			return 1;
		}

		public IComparer Comparator => Comparer<UInt16>.Default;

		#endregion

		public override System.Type PrimitiveClass => typeof(UInt16);

		public override object DefaultValue => ZeroObject;

		public override string ObjectToSQLString(object value, Dialect.Dialect dialect)
		{
			return ((ushort)value).ToString(CultureInfo.InvariantCulture);
		}
	}
}
