﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using NHibernate.Cfg.MappingSchema;
using NHibernate.Criterion;
using NHibernate.Driver;
using NHibernate.Engine;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2167
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : TestCaseMappingByCode
	{
		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			return TestDialect.SupportsComplexExpressionInGroupBy;
		}

		protected override bool AppliesTo(ISessionFactoryImplementor factory)
		{
			// When not using a named prefix, the driver use positional parameters, causing parameterized
			// expression used in group by and select to be not be considered as the same expression.
			return ((DriverBase) factory.ConnectionProvider.Driver).UseNamedPrefixInParameter;
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				var e1 = new Entity { Name = "Bob" };
				session.Save(e1);

				var e2 = new Entity { Name = "Sally" };
				session.Save(e2);

				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task GroupPropertyWithSqlFunctionDoesNotThrowAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var projection = session.CreateCriteria<Entity>().SetProjection(
					Projections.GroupProperty(
						Projections.SqlFunction(
							"substring",
							NHibernateUtil.String,
							Projections.Property("Name"),
							Projections.Constant(0),
							Projections.Constant(1))));

				await (projection.ListAsync());
			}
		}
	}
}
