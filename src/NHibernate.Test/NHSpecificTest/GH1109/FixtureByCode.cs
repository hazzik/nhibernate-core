using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1109
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Device>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.Name);
					rc.Bag(
						x => x.SpecialAttributes,
						m =>
						{
							m.Key(k => k.Column("DeviceId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
						},
						r => r.OneToMany());
				});

			mapper.Class<DeviceAttribValue>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Property(x => x.AttribValue);
					rc.ManyToOne(x => x.Device, m => m.Column("DeviceId"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var device1 = new Device { Name = "Zeta" };
			device1.SpecialAttributes.Add(new DeviceAttribValue { AttribValue = "A", Device = device1 });

			var device2 = new Device { Name = "Beta" };
			device2.SpecialAttributes.Add(new DeviceAttribValue { AttribValue = "B", Device = device2 });

			var device3 = new Device { Name = "Alpha" };
			device3.SpecialAttributes.Add(new DeviceAttribValue { AttribValue = "B", Device = device3 });

			session.Save(device1);
			session.Save(device2);
			session.Save(device3);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from DeviceAttribValue").ExecuteUpdate();
			session.CreateQuery("delete from Device").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void CanOrderByTwoColumnsWhenFirstComesFromLetWithCollectionSubquery()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var query = from c in session.Query<Device>()
						let x = c.SpecialAttributes.Where(a => a.AttribValue != null).SingleOrDefault()
						orderby x.AttribValue, c.Name
						select c;

			var result = query.ToList();

			Assert.That(result.Select(d => d.Name), Is.EqualTo(new[] { "Zeta", "Alpha", "Beta" }));

			transaction.Commit();
		}
	}
}
