using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2067
{
	namespace BaseAndDerivedClassProxies
	{
		//Base [Base] -> Derived1 [Derived1] -> Derived2 [Derived2]
		public class NotMappedBase
		{
		}

		public class Base : NotMappedBase
		{
			public virtual Guid Id { get; set; }
			public virtual int BaseName { get; set; }
		}

		public class Derived1 : Base
		{
			public virtual string Derived1Name { get; set; }
		}

		public class Derived2 : Derived1
		{
			public virtual string Derived2Name { get; set; }
		}

		[TestFixture]
		public class BaseAndDerivedClassProxiesFixture : TestCaseMappingByCode
		{
			private Guid _id = Guid.NewGuid();

			protected override HbmMapping GetMappings()
			{
				var mapper = new ModelMapper();
				mapper.Class<Base>(
					rc =>
					{
						rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
						rc.Property(x => x.BaseName);
					});

				mapper.JoinedSubclass<Derived1>(
					rc => { rc.Property(x => x.Derived1Name); });

				mapper.JoinedSubclass<Derived2>(
					rc => { rc.Property(x => x.Derived2Name); });

				return mapper.CompileMappingForAllExplicitlyAddedEntities();
			}

			[Test]
			public void ProxyForBase()
			{
				using (var s = OpenSession())
				{
					var b = s.Load<Base>(_id);
					Assert.That(b.Id, Is.EqualTo(_id));
				}
			}

			[Test]
			public void ProxyForDerived1()
			{
				using (var s = OpenSession())
				{
					var b = s.Load(typeof(Derived1), _id);

					Assert.That(b, Is.InstanceOf(typeof(Derived1)));
					Assert.That(b, Is.InstanceOf(typeof(Base)));
					Assert.That(((Derived1) b).Id, Is.EqualTo(_id));
				}
			}

			[Test]
			public void ProxyForDerived2()
			{
				using (var s = OpenSession())
				{
					var b = s.Load(typeof(Derived2), _id);

					Assert.That(b, Is.InstanceOf(typeof(Derived1)));
					Assert.That(b, Is.InstanceOf(typeof(Derived2)));
					Assert.That(b, Is.InstanceOf(typeof(Base)));
					Assert.That(((Derived2) b).Id, Is.EqualTo(_id));
				}
			}
		}
	}
}
