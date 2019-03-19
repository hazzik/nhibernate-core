using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2067
{
	namespace BaseClassAndDerivedInterfaceProxies
	{
		//Base [Base] -> Derived1 [IDerived1] -> Derived2 [IDerived2]
		public class NotMappedBase
		{
		}

		public class Base : NotMappedBase
		{
			public virtual Guid Id { get; set; }
			public virtual int BaseName { get; set; }
		}

		public class Derived1Interface : Base, IDerived1
		{
			public virtual string Derived1Name { get; set; }
		}

		public class Derived2Interface : Derived1Interface, IDerived2
		{
			public virtual string Derived2Name { get; set; }
		}

		public interface IDerived1
		{
			Guid Id { get; set; }
			string Derived1Name { get; set; }
		}

		public interface IDerived2
		{
			Guid Id { get; set; }
			string Derived1Name { get; set; }
		}

		[TestFixture]
		public class BaseClassAndDerivedInterfaceProxiesFixture : TestCaseMappingByCode
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

				mapper.JoinedSubclass<Derived1Interface>(
					rc =>
					{
						rc.Proxy(typeof(IDerived1));
						rc.Property(x => x.Derived1Name);
					});

				mapper.JoinedSubclass<Derived2Interface>(
					rc =>
					{
						rc.Proxy(typeof(IDerived2));
						rc.Property(x => x.Derived2Name);
					});

				return mapper.CompileMappingForAllExplicitlyAddedEntities();
			}

			[Test]
			public void ProxyForBaseClass()
			{
				using (var s = OpenSession())
				{
					var b = s.Load<Base>(_id);
					Assert.That(b.Id, Is.EqualTo(_id));
				}
			}

			[Test]
			public void ProxyForDerived1Interface()
			{
				using (var s = OpenSession())
				{
					var b = s.Load(typeof(Derived1Interface), _id);

					Assert.That(b, Is.InstanceOf(typeof(IDerived1)));
//					Assert.That(b, Is.InstanceOf(typeof(Base)));
					Assert.That(((IDerived1) b).Id, Is.EqualTo(_id));
				}
			}

			[Test]
			public void ProxyForDerived2Interface()
			{
				using (var s = OpenSession())
				{
					var b = s.Load(typeof(Derived2Interface), _id);

					Assert.That(b, Is.InstanceOf(typeof(IDerived2)));
//					Assert.That(b, Is.InstanceOf(typeof(IDerived1)));
//					Assert.That(b, Is.InstanceOf(typeof(Base)));
					Assert.That(((IDerived2) b).Id, Is.EqualTo(_id));
				}
			}
		}
	}
}
