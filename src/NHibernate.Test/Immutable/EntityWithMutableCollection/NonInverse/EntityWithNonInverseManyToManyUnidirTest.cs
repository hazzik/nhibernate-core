using System;
using NHibernate.Test.Immutable.EntityWithMutableCollection;
using NUnit.Framework;

namespace NHibernate.Test.Immutable.EntityWithMutableCollection.NonInverse
{
	[TestFixture]
	public class EntityWithNonInverseManyToManyUnidirTest : AbstractEntityWithManyToManyTest
	{
		protected override string[] Mappings
		{
			get
			{
				return new string[] { "Immutable.EntityWithMutableCollection.NonInverse.ContractVariationUnidir.hbm.xml" };
			}
		}
	}
}
