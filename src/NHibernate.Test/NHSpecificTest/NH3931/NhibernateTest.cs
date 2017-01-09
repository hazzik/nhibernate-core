using System;
using System.Collections.Generic;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3931
{
	public class Child
	{
		public virtual Guid Id { get; set; }
		public virtual string Title { get; set; }
	}

	public class Parent
	{
		public virtual Guid Id { get; set; }
		public virtual string Title { get; set; }
		public virtual ICollection<Child> Child { get; set; }
	}

	public class ParentA : Parent
	{
	}

	public class ParentB : Parent
	{
	}

	[TestFixture]
	public class NhibernateTest : TestCaseMappingByCode
	{
		[Test]
		public void SaveTest()
		{
			var a = new ParentA
			{
				Title = "A",
				Child = new List<Child> {new Child {Title = "Child A"}}
			};

			var b = new ParentB
			{
				Title = "B",
				Child = new List<Child> {new Child {Title = "Child B"}}
			};

			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Save(b);
				session.Save(a);
				//session.Flush();
				tx.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var tx = session.BeginTransaction())
			{
				session.Delete("from System.Object");
				tx.Commit();
			}
		}

		protected override void Configure(Configuration configuration)
		{
			configuration
				.SetProperty(Cfg.Environment.OrderInserts, "true")
				.SetProperty(Cfg.Environment.OrderUpdates, "true")
				;
		}

		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Child>(
				rc =>
				{
					rc.Id(t => t.Id, im => im.Generator(Generators.Guid));
					rc.Property(t => t.Title);
				});
			mapper.Class<Parent>(
				rc =>
				{
					//rc.Lazy(false);
					rc.Id(t => t.Id, im => im.Generator(Generators.Guid));
					rc.Discriminator(t => t.Column("ParentType"));
					rc.DiscriminatorValue("Parent");
					rc.Set(
						t => t.Child,
						bm =>
						{
							bm.Lazy(CollectionLazy.NoLazy);
							bm.Key(
								km =>
								{
									km.NotNullable(true);
									km.Update(false);
								});
							bm.Inverse(false);
							bm.Cascade(Mapping.ByCode.Cascade.All);
						},
						rm => rm.OneToMany());
				});

			mapper.Subclass<ParentA>(
				rc =>
				{
					rc.Lazy(false);
					rc.DiscriminatorValue("ParentA");
				});
			mapper.Subclass<ParentB>(
				rc =>
				{
					rc.Lazy(false);
					rc.DiscriminatorValue("ParentB");
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}
	}
}
