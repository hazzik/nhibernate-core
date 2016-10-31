using System;
using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3100
{
	public class Entity
	{
		public virtual Guid Id { get; set; }
		public virtual bool? Flag { get; set; }
	}

	public class NullableBooleanFixture : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();
			mapper.Class<Entity>(rc =>
			{
				rc.Id(x => x.Id, m => m.Generator(Generators.GuidComb));
				rc.Property(x => x.Flag);
			});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Save(new Entity {Flag = true});
				session.Save(new Entity {Flag = false});
				session.Save(new Entity {Flag = null});
				session.Flush();
				transaction.Commit();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
				session.Delete("from System.Object");

				session.Flush();
				transaction.Commit();
			}
		}

		[Test]
		public void QueryWhereFlagIsTrue()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag == true).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagIsFalse()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag == false).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagIsNull()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag == null).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagIsNotTrue()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag != true).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFlagIsNotFalse()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag != false).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFlagIsNotNull()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag != null).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFlagEqualsToIteself()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => e.Flag == e.Flag).ToList();

				Assert.That(result, Has.Count.EqualTo(3));
			}
		}

		[Test]
		public void QueryWhereFlagDoesNotEqualToItself()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => e.Flag != e.Flag).ToList();

				Assert.That(result, Is.Empty);
			}
		}

		[Test]
		public void QueryWhereFlagEqualsToCondtion()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => e.Flag == (e.Id != null)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagDoesNotEqualToCondtion()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => e.Flag != (e.Id != null)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFlagEqualsToTrue()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag.Equals(true)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagEqualsToFalse()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag.Equals(false)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagEqualsToNull()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag.Equals(null)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagDoesNotEqualToTrue()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => !e.Flag.Equals(true)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFlagDoesNotEqualToFalse()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => !e.Flag.Equals(false)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFlagDoesNotEqualToNull()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => !e.Flag.Equals(null)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFlagEqualsToIteself2()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => e.Flag.Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(3));
			}
		}

		[Test]
		public void QueryWhereFlagDoesNotEqualToItself2()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => !e.Flag.Equals(e.Flag)).ToList();

				Assert.That(result, Is.Empty);
			}
		}

		[Test]
		public void QueryWhereFlagEqualsToCondtion2()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => e.Flag.Equals(e.Id != null)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFlagDoesNotEqualToCondtion2()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => !e.Flag.Equals(e.Id != null)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereTrueEqualsToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => true.Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereFalseEqualsToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => false.Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void X()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
			    var result = session.Query<Entity>().Where(e => e.Id == Guid.Empty).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereNullEqualsToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => default(bool?).Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereTrueDoesNotEqualToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => !true.Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereFalseDoesNotEqualToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => !false.Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereNullDoesNotEqualToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => !default(bool?).Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void QueryWhereConditionEqualsToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => (e.Id != null).Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void QueryWhereCondtionDoesNotEqualToFlag()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				// ReSharper disable once EqualExpressionComparison
				var result = session.Query<Entity>().Where(e => !(e.Id != null).Equals(e.Flag)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}

		[Test]
		public void GetValueOrDefault()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag.GetValueOrDefault()).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void GetValueOrDefaultFalse()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag.GetValueOrDefault(false)).ToList();

				Assert.That(result, Has.Count.EqualTo(1));
			}
		}

		[Test]
		public void GetValueOrDefaultTrue()
		{
			using (var session = OpenSession())
			using (session.BeginTransaction())
			{
				var result = session.Query<Entity>().Where(e => e.Flag.GetValueOrDefault(true)).ToList();

				Assert.That(result, Has.Count.EqualTo(2));
			}
		}
	}
}