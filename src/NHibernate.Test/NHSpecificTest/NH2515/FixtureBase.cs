using System;
using System.Collections;

namespace NHibernate.Test.NHSpecificTest.NH2515
{
	public abstract class FixtureBase : TestCase
	{
		protected override IList Mappings
		{
			get
			{
				return new[]
					{
						string.Format("NHSpecificTest.{0}.{1}.Mappings.hbm.xml", Part(typeof (FixtureBase)), Part(GetType()))
					};
			}
		}

		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		private static string Part(System.Type type)
		{
			var ns = type.Namespace;
			return ns.Substring(ns.LastIndexOf('.') + 1);
		}

		protected static int CountDeletes(SqlLogSpy spy)
		{
			return spy.GetWholeLog()
					   .ToLowerInvariant()
					   .Split(new[] {"delete"}, StringSplitOptions.RemoveEmptyEntries)
					   .Length - 1;
		}
	}
}
