using System;
using System.Linq;
using NHibernate.Exceptions;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1472
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			// A customer without any contact: the projected DateOfBirth will be null.
			var customer = new Customer { Name = "Bob" };
			customer.Purchases.Add(new Purchase { Customer = customer });
			session.Save(customer);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Purchase").ExecuteUpdate();
			session.CreateQuery("delete from Customer").ExecuteUpdate();
			session.CreateQuery("delete from Contact").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void ProjectingNullValueOnNonNullableTypeThrowsMeaningfulException()
		{
			using var session = OpenSession();

			var query = session
				.Query<Customer>()
				.Select(c => new {c.Name, BirthDate = c.BillingContact.DateOfBirth});

			var exception = Assert.Throws<GenericADOException>(() => query.ToList());
			Assert.That(exception.InnerException, Is.TypeOf<InvalidCastException>());
			Assert.That(
				exception.InnerException.Message,
				Does.Contain("'BirthDate'"),
				"The failing projection should be named after the member it is assigned to.");
			Assert.That(
				exception.InnerException.Message,
				Does.Contain(typeof(DateTime).FullName),
				"The target type should be named.");
		}

		[Test]
		public void ProjectingNullValueOnNullableTypeYieldsNull()
		{
			using var session = OpenSession();

			var result = session
				.Query<Customer>()
				.Select(c => new {c.Name, DateOfBirth = (DateTime?) c.BillingContact.DateOfBirth})
				.Single();

			Assert.That(result.DateOfBirth, Is.Null);
		}

		[Test]
		public void ProjectingNullValueOnNonNullableTypeInNestedSelectThrowsMeaningfulException()
		{
			using var session = OpenSession();

			var query = session
				.Query<Customer>()
				.Select(
					c => new
					{
						c.Name,
						Purchases = c.Purchases.Select(p => new {BirthDate = p.DeliveryContact.DateOfBirth})
					});

			var exception = Assert.Throws<GenericADOException>(() => query.ToList());
			Assert.That(exception.InnerException, Is.TypeOf<InvalidCastException>());
			Assert.That(
				exception.InnerException.Message,
				Does.Contain("'BirthDate'"),
				"The failing projection should be named after the member it is assigned to.");
		}
	}
}
