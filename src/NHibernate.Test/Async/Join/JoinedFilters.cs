﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by AsyncGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace NHibernate.Test.Join
{
	using System.Threading.Tasks;
	using System.Threading;
	[TestFixture]
	public class JoinedFiltersAsync : TestCase
	{
		protected override string[] Mappings
		{
			get
			{
				return new[]
				{
					"Join.TennisPlayer.hbm.xml",
					"Join.Person.hbm.xml"
				};
			}
		}

		protected override void OnTearDown()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				s.Delete("from TennisPlayer");
				tx.Commit();
			}
		}

		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		[Test]
		public async Task FilterOnPrimaryTableAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				s.EnableFilter("NameFilter").SetParameter("name", "Nadal");

				await (CreatePlayersAsync(s));

				IList<TennisPlayer> people = await (s.CreateCriteria<TennisPlayer>().ListAsync<TennisPlayer>());
				Assert.AreEqual(1, people.Count);
				Assert.AreEqual("Nadal", people[0].Name);

				await (tx.CommitAsync());
			}
		}

		[Test]
		public async Task FilterOnJoinedTableAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				s.EnableFilter("MakeFilter").SetParameter("make", "Babolat");

				await (CreatePlayersAsync(s));

				IList<TennisPlayer> people = await (s.CreateCriteria<TennisPlayer>().ListAsync<TennisPlayer>());
				Assert.AreEqual(1, people.Count);
				Assert.AreEqual("Babolat", people[0].RacquetMake);

				await (tx.CommitAsync());
			}
		}

		[Test]
		public async Task FilterOnJoinedTableWithRepeatedColumnAsync()
		{
			using (ISession s = OpenSession())
			using (ITransaction tx = s.BeginTransaction())
			{
				s.EnableFilter("ModelFilter").SetParameter("model", "AeroPro Drive");

				await (CreatePlayersAsync(s));

				IList<TennisPlayer> people = await (s.CreateCriteria<TennisPlayer>().ListAsync<TennisPlayer>());
				Assert.AreEqual(1, people.Count);
				Assert.AreEqual("AeroPro Drive", people[0].RacquetModel);

				await (tx.CommitAsync());
			}
		}

		private static async Task CreatePlayersAsync(ISession s, CancellationToken cancellationToken = default(CancellationToken))
		{
			await (CreateAndSavePlayerAsync(s, "Nadal", "Babolat", "AeroPro Drive", cancellationToken));
			await (CreateAndSavePlayerAsync(s, "Federer", "Wilson", "Six.One Tour BLX", cancellationToken));
			await (s.FlushAsync(cancellationToken));
		}

		private static Task CreateAndSavePlayerAsync(ISession session, string name, string make, string model, CancellationToken cancellationToken = default(CancellationToken))
		{
			try
			{
				var s = new TennisPlayer() { Name = name, RacquetMake = make, RacquetModel = model };
				return session.SaveAsync(s, cancellationToken);
			}
			catch (Exception ex)
			{
				return Task.FromException<object>(ex);
			}
		}
	}
}
