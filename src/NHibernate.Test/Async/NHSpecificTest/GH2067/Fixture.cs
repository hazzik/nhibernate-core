﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NHibernate.Proxy;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH2067
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : TestCaseMappingByCode
	{
		private object domesticCatId;
		private object catId;

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Cat>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Name);
			});

			mapper.JoinedSubclass<DomesticCat>(rc =>
			{
				rc.Proxy(typeof(IDomesticCat));
				rc.Property(x => x.OwnerName);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				catId = session.Save(new Cat { Name = "Bob" });

				domesticCatId = session.Save(new DomesticCat {Name = "Tom", OwnerName = "Jerry"});

				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				// HQL Delete of entities with joins require temp tables, which are not
				// supported by all dialects: use in memory-delete instead.
				session.Delete("from System.Object");

				transaction.Commit();
			}
		}

		[Test]
		public async Task CanLoadDomesticCatUsingBaseClassAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var cat = await (session.LoadAsync<Cat>(domesticCatId));
				Assert.That(cat, Is.Not.Null);
				Assert.That(cat.Name, Is.EqualTo("Tom"));
				var domesticCat = (IDomesticCat) cat;
				Assert.That(domesticCat.Name, Is.EqualTo("Tom"));
				Assert.That(domesticCat.OwnerName, Is.EqualTo("Jerry"));
				var proxy = (INHibernateProxy) cat;
				Assert.That(proxy.HibernateLazyInitializer.PersistentClass, Is.EqualTo(typeof(Cat)));
			}
		}

		[Test]
		public async Task CanLoadDomesticCatUsingBaseClassInterfaceAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var cat = await (session.LoadAsync<ICat>(domesticCatId));
				Assert.That(cat, Is.Not.Null);
				Assert.That(cat.Name, Is.EqualTo("Tom"));
				var domesticCat = (IDomesticCat) cat;
				Assert.That(domesticCat.Name, Is.EqualTo("Tom"));
				Assert.That(domesticCat.OwnerName, Is.EqualTo("Jerry"));
				var proxy = (INHibernateProxy) cat;
				Assert.That(proxy.HibernateLazyInitializer.PersistentClass, Is.EqualTo(typeof(Cat)));
			}
		}

		[Test]
		public async Task CanLoadDomesticCatUsingInterfaceAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var cat = await (session.LoadAsync<IDomesticCat>(domesticCatId));
				Assert.That(cat, Is.Not.Null);
				Assert.That(cat.Name, Is.EqualTo("Tom"));
				Assert.That(cat.OwnerName, Is.EqualTo("Jerry"));
				var proxy = (INHibernateProxy) cat;
				Assert.That(proxy.HibernateLazyInitializer.PersistentClass, Is.EqualTo(typeof(DomesticCat)));
			}
		}

		[Test]
		public void ThrowWhenTryToLoadDomesticCatUsingSealedClassAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				Assert.ThrowsAsync<InvalidCastException>(() => session.LoadAsync<DomesticCat>(domesticCatId));
			}
		}

		[Test]
		public async Task CanLoadDomesticCatUsingSealedClassAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var cat = (IDomesticCat) await (session.LoadAsync(typeof(DomesticCat), domesticCatId));
				Assert.That(cat, Is.Not.Null);
				Assert.That(cat.Name, Is.EqualTo("Tom"));
				Assert.That(cat.OwnerName, Is.EqualTo("Jerry"));
				var proxy = (INHibernateProxy) cat;
				Assert.That(proxy.HibernateLazyInitializer.PersistentClass, Is.EqualTo(typeof(DomesticCat)));
			}
		}

		[Test]
		public async Task CanLoadDomesticCatUsingSideInterfaceAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var cat = await (session.LoadAsync<IPet>(domesticCatId));
				Assert.That(cat, Is.Not.Null);
				Assert.That(cat.OwnerName, Is.EqualTo("Jerry"));
				var proxy = (INHibernateProxy) cat;
				Assert.That(proxy.HibernateLazyInitializer.PersistentClass, Is.EqualTo(typeof(DomesticCat)));
			}
		}

		[Test]
		public async Task CanLoadCatUsingClassAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var cat = await (session.LoadAsync<Cat>(catId));
				Assert.That(cat, Is.Not.Null);
				Assert.That(cat.Name, Is.EqualTo("Bob"));
				var proxy = (INHibernateProxy) cat;
				Assert.That(proxy.HibernateLazyInitializer.PersistentClass, Is.EqualTo(typeof(Cat)));
			}
		}
		
		[Test]
		public async Task CanLoadCatUsingInterfaceAsync()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var cat = await (session.LoadAsync<ICat>(catId));
				Assert.That(cat, Is.Not.Null);
				Assert.That(cat.Name, Is.EqualTo("Bob"));
				var proxy = (INHibernateProxy) cat;
				Assert.That(proxy.HibernateLazyInitializer.PersistentClass, Is.EqualTo(typeof(Cat)));
			}
		}
	}
}
