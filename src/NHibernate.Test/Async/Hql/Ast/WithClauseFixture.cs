﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System.Collections;
using NHibernate.Hql.Ast.ANTLR;
using NUnit.Framework;

namespace NHibernate.Test.Hql.Ast
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class WithClauseFixtureAsync : BaseFixture
	{
		public ISession OpenNewSession()
		{
			return OpenSession();
		}

		protected override bool AppliesTo(Dialect.Dialect dialect)
		{
			// Some classes are mapped with table joins, which requires temporary tables for DML to work,
			// and DML is used for the cleanup.
			return Dialect.SupportsTemporaryTables;
		}

		[Test]
		public async Task WithClauseFailsWithFetchAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());

			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();

			Assert.ThrowsAsync<SemanticException>(
			  () =>
				s.CreateQuery("from Animal a inner join fetch a.offspring as o with o.bodyWeight = :someLimit").SetDouble(
					"someLimit", 1).ListAsync(), "ad-hoc on clause allowed with fetched association");

			await (txn.CommitAsync());
			s.Close();

			await (data.CleanupAsync());
		}

		[Test]
		public async Task WithClauseOnSubclassesAsync()
		{
			using (var s = OpenSession())
			{
				await (s.CreateQuery(
					"from Animal a inner join a.offspring o inner join o.mother as m inner join m.father as f with o.bodyWeight > 1").ListAsync());

				// f.bodyWeight is a reference to a column on the Animal table; however, the 'f'
				// alias relates to the Human.friends collection which the aonther Human entity.
				// Group join allows us to use such constructs
				await (s.CreateQuery("from Human h inner join h.friends as f with f.bodyWeight < :someLimit").SetDouble("someLimit", 1).ListAsync());

				await (s.CreateQuery("from Human h inner join h.offspring o with o.mother.father = :cousin")
				.SetInt32("cousin", 123)
				.ListAsync());
			}
		}

		[Test]
		public async Task WithClauseAsync()
		{
			var data = new TestData(this);
			await (data.PrepareAsync());

			ISession s = OpenSession();
			ITransaction txn = s.BeginTransaction();

			// one-to-many
			IList list =
			await (s.CreateQuery("from Human h inner join h.offspring as o with o.bodyWeight < :someLimit").SetDouble("someLimit", 1).
				ListAsync());
			Assert.That(list, Is.Empty, "ad-hoc on did not take effect");

			// many-to-one
			list =
				await (s.CreateQuery("from Animal a inner join a.mother as m with m.bodyWeight < :someLimit").SetDouble("someLimit", 1).
					ListAsync());
			Assert.That(list, Is.Empty, "ad-hoc on did not take effect");

			// many-to-many
			list = await (s.CreateQuery("from Human h inner join h.friends as f with f.nickName like 'bubba'").ListAsync());
			Assert.That(list, Is.Empty, "ad-hoc on did not take effect");

			await (txn.CommitAsync());
			s.Close();

			await (data.CleanupAsync());
		}

		private class TestData
		{
			private readonly WithClauseFixtureAsync tc;

			public TestData(WithClauseFixtureAsync tc)
			{
				this.tc = tc;
			}

			public async Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken))
			{
				ISession session = tc.OpenNewSession();
				ITransaction txn = session.BeginTransaction();

				var mother = new Human { BodyWeight = 10, Description = "mother" };

				var father = new Human { BodyWeight = 15, Description = "father" };

				var child1 = new Human { BodyWeight = 5, Description = "child1" };

				var child2 = new Human { BodyWeight = 6, Description = "child2" };

				var friend = new Human { BodyWeight = 20, Description = "friend" };

				child1.Mother = mother;
				child1.Father = father;
				mother.AddOffspring(child1);
				father.AddOffspring(child1);

				child2.Mother = mother;
				child2.Father = father;
				mother.AddOffspring(child2);
				father.AddOffspring(child2);

				father.Friends = new[] { friend };

				await (session.SaveAsync(mother, cancellationToken));
				await (session.SaveAsync(father, cancellationToken));
				await (session.SaveAsync(child1, cancellationToken));
				await (session.SaveAsync(child2, cancellationToken));
				await (session.SaveAsync(friend, cancellationToken));

				await (txn.CommitAsync(cancellationToken));
				session.Close();
			}

			public async Task CleanupAsync(CancellationToken cancellationToken = default(CancellationToken))
			{
				ISession session = tc.OpenNewSession();
				ITransaction txn = session.BeginTransaction();
				await (session.CreateQuery("delete Animal where mother is not null").ExecuteUpdateAsync(cancellationToken));
				IList humansWithFriends = await (session.CreateQuery("from Human h where exists(from h.friends)").ListAsync(cancellationToken));
				foreach (var friend in humansWithFriends)
				{
					await (session.DeleteAsync(friend, cancellationToken));
				}
				await (session.CreateQuery("delete Animal").ExecuteUpdateAsync(cancellationToken));
				await (txn.CommitAsync(cancellationToken));
				session.Close();
			}
		}
	}
}
