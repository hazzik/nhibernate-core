using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1273
{
	[TestFixture]
	public class Fixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Operation>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
				rc.Bag(
					x => x.Steps,
					m =>
					{
						m.Key(k => k.Column("OperationId"));
						m.Cascade(Mapping.ByCode.Cascade.All);
						m.OrderBy(s => s.Order);
					},
					r => r.OneToMany());
			});

			mapper.Class<OperationStep>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
				// The column is explicitly quoted: it collides with the "Order" SQL keyword.
				rc.Property(x => x.Order, m => m.Column("`Order`"));
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var operation = new Operation();
			operation.Steps.Add(new OperationStep { Order = 2 });
			operation.Steps.Add(new OperationStep { Order = 1 });
			session.Save(operation);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from OperationStep").ExecuteUpdate();
			session.CreateQuery("delete from Operation").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanFetchBagOrderedByQuotedColumn()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var operations = session.Query<Operation>().Fetch(o => o.Steps).ToList();

			Assert.That(operations, Has.Count.EqualTo(1));
			Assert.That(operations[0].Steps.Select(s => s.Order), Is.EqualTo(new[] { 1, 2 }));
		}
	}
}
