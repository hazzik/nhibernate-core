using NHibernate.Linq.Functions;

namespace NHibernate.Test.NHSpecificTest.GH1164
{
	public class MyLinqToHqlGeneratorsRegistry : DefaultLinqToHqlGeneratorsRegistry
	{
		public MyLinqToHqlGeneratorsRegistry()
		{
			RegisterGenerator(typeof(MyEntity).GetMethod(nameof(MyEntity.ToString), System.Type.EmptyTypes), new CustomToStringGenerator());
		}
	}
}
