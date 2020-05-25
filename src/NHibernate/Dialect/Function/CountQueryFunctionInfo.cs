using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Dialect.Function
{
	[Serializable]
	class CountQueryFunctionInfo : ClassicAggregateFunction
	{
		public CountQueryFunctionInfo() : base("count", true) { }

		public override IType GetReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError)
		{
			return NHibernateUtil.Int64;
		}
	}
}
