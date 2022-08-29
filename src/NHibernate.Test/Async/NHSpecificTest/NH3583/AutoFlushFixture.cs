﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Linq;
using System.Transactions;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3583
{
	using System.Threading.Tasks;
	[TestFixture]
	public class AutoFlushFixtureAsync : TestCaseMappingByCode
	{
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
		public async Task ShouldAutoFlushWhenInExplicitTransactionAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var e1 = new Entity { Name = "Bob" };
				await (session.SaveAsync(e1));

				var result = await ((from e in session.Query<Entity>()
							  where e.Name == "Bob"
							  select e).ToListAsync());

				Assert.That(result.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public async Task ShouldAutoFlushWhenInDistributedTransactionAsync()
		{
			Assume.That(Sfi.ConnectionProvider.Driver.SupportsSystemTransactions, Is.True);

			using (new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
			using (var session = OpenSession())
			{
				var e1 = new Entity { Name = "Bob" };
				await (session.SaveAsync(e1));

				var result = await ((from e in session.Query<Entity>()
							  where e.Name == "Bob"
							  select e).ToListAsync());

				Assert.That(result.Count, Is.EqualTo(1));
			}
		}
	}
}
