﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3316
{
	using System.Threading.Tasks;
	[TestFixture]
	public class ByCodeFixtureAsync : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(e =>
			{
				e.Id(x => x.Id, id => id.Generator(Generators.GuidComb));
				e.Set(x => x.Children, c =>
				{
					c.Key(key => key.Column("EntityId"));
					c.Cascade(NHibernate.Mapping.ByCode.Cascade.All | NHibernate.Mapping.ByCode.Cascade.DeleteOrphans);
				},
				r =>
				{
					r.Component(c =>
					{
						c.Parent(x => x.Parent);
						c.Property(x => x.Value, pm => pm.Column("`Value`"));
					});
				});
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnTearDown()
		{
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public async Task Test_That_Parent_Property_Can_Be_Persisted_And_RetrievedAsync()
		{
			Guid id = Guid.Empty;
			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				Entity e = new Entity();
				e.AddChild(1);
				e.AddChild(2);

				await (session.SaveAsync(e));
				await (session.FlushAsync());
				await (transaction.CommitAsync());
				id = e.Id;
			}

			using (ISession session = OpenSession())
			using (ITransaction transaction = session.BeginTransaction())
			{
				Entity e = await (session.GetAsync<Entity>(id));
				Assert.AreEqual(2, e.Children.Count());
				foreach (ChildComponent c in e.Children)
					Assert.AreEqual(e, c.Parent);

				await (session.FlushAsync());
				await (transaction.CommitAsync());
			}
		}
	}
}
