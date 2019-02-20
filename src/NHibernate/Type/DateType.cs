using System;
using System.Data;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace NHibernate.Type
{
	/// <summary>
	/// Maps the Year, Month, and Day of a <see cref="System.DateTime"/> Property to a 
	/// <see cref="DbType.Date"/> column
	/// </summary>
	[Serializable]
	public class DateType : AbstractDateTimeType
	{
		/// <summary>Default constructor</summary>
		public DateType() : base(SqlTypeFactory.Date)
		{
		}

		/// <inheritdoc />
		public override string Name => "Date";

		/// <inheritdoc />
		protected override DateTime AdjustDateTime(DateTime dateValue) =>
			dateValue.Date;

		/// <inheritdoc />
		public override bool IsEqual(object x, object y)
		{
			if (x == y)
			{
				return true;
			}
			if (x == null || y == null)
			{
				return false;
			}

			var date1 = (DateTime)x;
			var date2 = (DateTime)y;
			if (date1.Equals(date2))
				return true;

			return date1.Day == date2.Day
				 && date1.Month == date2.Month
				 && date1.Year == date2.Year;
		}

		/// <inheritdoc />
		public override int GetHashCode(object x)
		{
			var date = (DateTime)x;
			var hashCode = 1;
			unchecked
			{
				hashCode = 31 * hashCode + date.Day;
				hashCode = 31 * hashCode + date.Month;
				hashCode = 31 * hashCode + date.Year;
			}
			return hashCode;
		}

		/// <inheritdoc />
		public override string ToLoggableString(object value, ISessionFactoryImplementor factory)
		{
			return (value == null) ? null :
				// 6.0 TODO: inline this call.
#pragma warning disable 618
				ToString(value);
#pragma warning restore 618
		}

		/// <inheritdoc />
		// Since 5.2
		[Obsolete("This method has no more usages and will be removed in a future version. Override ToLoggableString instead.")]
		public override string ToString(object val) =>
			((DateTime) val).ToShortDateString();

		/// <inheritdoc />
		public override object DefaultValue => DateTime.MinValue;

		/// <inheritdoc />
		public override string ObjectToSQLString(object value, Dialect.Dialect dialect) =>
			"\'" + ((DateTime)value).ToShortDateString() + "\'";
	}
}
