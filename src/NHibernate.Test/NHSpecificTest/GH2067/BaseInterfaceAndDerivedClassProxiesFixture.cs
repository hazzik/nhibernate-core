using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2067
{
	namespace BaseInterfaceAndDerivedClassProxies
	{
		//Base [IBase] -> Derived1 [Derived1] -> Derived2 [Derived2]
		public class NotMappedBase
		{
		}

		public class BaseInterface : NotMappedBase, IBase
		{
			public virtual Guid Id { get; set; }
			public virtual int BaseName { get; set; }
		}

		public class Derived1 : BaseInterface
		{
			public virtual string Derived1Name { get; set; }
		}

		public class Derived2 : Derived1
		{
			public virtual string Derived2Name { get; set; }
		}

		public interface IBase
		{
			Guid Id { get; set; }
			int BaseName { get; set; }
		}

		[TestFixture]
		public class BaseInterfaceAndDerivedClassProxiesFixture : TestCaseMappingByCode
		{
			private Guid _id = Guid.NewGuid();

			protected override HbmMapping GetMappings()
			{
				var mapper = new ModelMapper();
				mapper.Class<BaseInterface>(
					rc =>
					{
						rc.Proxy(typeof(IBase));
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
					var b = s.Load(typeof(IBase), _id);
					Assert.That(((IBase) b).Id, Is.EqualTo(_id));
				}
			}

			[Test]
			public void ProxyForDerived1()
			{
				using (var s = OpenSession())
				{
					var b = s.Load(typeof(Derived1), _id);

					Assert.That(b, Is.InstanceOf(typeof(Derived1)));
					Assert.That(b, Is.InstanceOf(typeof(IBase)));
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
					Assert.That(b, Is.InstanceOf(typeof(IBase)));
					Assert.That(((Derived2) b).Id, Is.EqualTo(_id));
				}
			}
		}
	}
}
