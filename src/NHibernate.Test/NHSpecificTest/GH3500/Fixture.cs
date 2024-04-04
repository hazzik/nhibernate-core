using System.Collections.Generic;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH3500;

[TestFixture]
public class Fixture : TestCaseMappingByCode
{
	protected override HbmMapping GetMappings()
	{
		var mapper = new ModelMapper();
		mapper.Class<Entity>(rc =>
		{
			rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
			rc.Property(x => x.GroupId1);
			rc.Property(x => x.GroupId2);
		});

		return mapper.CompileMappingForAllExplicitlyAddedEntities();
	}

	protected override void OnSetUp()
	{
		using var session = OpenSession();
		using var transaction = session.BeginTransaction();

		for (var i = 0; i < 10; i++)
		{
			session.Save(new Entity { GroupId1 = i, GroupId2 = 10 + i });
		}

		transaction.Commit();
	}

	protected override void OnTearDown()
	{
		using var session = OpenSession();
		using var transaction = session.BeginTransaction();

		session.CreateQuery("delete from System.Object").ExecuteUpdate();

		transaction.Commit();
	}

	[Test]
	public void CanQuerySameListTwice()
	{
		using var session = OpenSession();
		using var transaction = session.BeginTransaction();

		List<int> groups = [7, 14, 21, 28];

		using SqlLogSpy spy = new();
		var result = session
		             .Query<Entity>()
		             .Count(
			             e => e.GroupId1.HasValue && groups.Contains(e.GroupId1.Value) ||
			                  e.GroupId2.HasValue && groups.Contains(e.GroupId2.Value));

		Assert.That(result, Is.EqualTo(2));
		var log = spy.GetWholeLog();
		Assert.Multiple(
			() =>
			{
				Assert.That(log, Does.Contain("p0"));
				Assert.That(log, Does.Contain("p1"));
				Assert.That(log, Does.Contain("p2"));
				Assert.That(log, Does.Contain("p3"));
				
				Assert.That(log, Does.Not.Contain("p4"));
				Assert.That(log, Does.Not.Contain("p5"));
				Assert.That(log, Does.Not.Contain("p6"));
				Assert.That(log, Does.Not.Contain("p7"));
			});

		transaction.Commit();
	}
}
