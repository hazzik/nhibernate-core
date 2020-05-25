using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Dialect.Function
{
	/// <summary>
	/// Classic COUNT sqlfunction that return types as it was done in Hibernate 3.1
	/// </summary>
	[Serializable]
	public class ClassicCountFunction : ClassicAggregateFunction
	{
		public ClassicCountFunction() : base("count", true)
		{
		}

		public override IType GetReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError)
		{
			return NHibernateUtil.Int32;
		}
	}
}
