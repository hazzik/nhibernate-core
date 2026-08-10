using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1083
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Order>(
				rc =>
				{
					// Table name is plural, like in the original report, to avoid colliding
					// with the "order" SQL keyword at the DDL/DML level. The HQL entity name
					// ("Order") is what collides with the HQL "order" keyword.
					rc.Table("Orders");
					rc.Id(x => x.Id, m => m.Generator(Generators.Assigned));
					rc.Property(x => x.IsConfirmed);
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.Save(new Order { Id = 1, IsConfirmed = false });
			session.Save(new Order { Id = 2, IsConfirmed = false });

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			// Bulk HQL delete on this entity hits the same bug as the bulk update under test,
			// so clean up one row at a time instead.
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			foreach (var order in session.Query<Order>().ToList())
			{
				session.Delete(order);
			}

			transaction.Commit();
		}

		[Test]
		public void BulkUpdateOnEntityNamedAfterHqlKeyword()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var ids = new List<int> { 1, 2 };

			var updateCount = session
				.CreateQuery("update Order set IsConfirmed = :isConfirmed where Id in (:ids)")
				.SetParameter("isConfirmed", true)
				.SetParameterList("ids", ids)
				.ExecuteUpdate();

			Assert.That(updateCount, Is.EqualTo(2));

			transaction.Commit();

			using var verifySession = OpenSession();
			var confirmedCount = verifySession.Query<Order>().Count(o => o.IsConfirmed);
			Assert.That(confirmedCount, Is.EqualTo(2));
		}
	}
}
