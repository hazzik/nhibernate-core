using System;
using System.Collections.Generic;
using NHibernate.Engine;
using NHibernate.Type;

namespace NHibernate.Dialect.Function
{
	[Serializable]
	class AvgQueryFunctionInfo : ClassicAggregateFunction
	{
		public AvgQueryFunctionInfo() : base("avg", false) { }


		/// <inheritdoc />
		public override IType GetReturnType(IEnumerable<IType> argumentTypes, IMapping mapping, bool throwOnError)
		{
			if (!TryGetArgumentType(argumentTypes, mapping, throwOnError, out _, out _))
			{
				return null;
			}

			return NHibernateUtil.Double;
		}
	}
}
