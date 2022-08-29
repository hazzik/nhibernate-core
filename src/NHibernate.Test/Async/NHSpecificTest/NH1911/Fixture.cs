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
using NHibernate.Criterion;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH1911
{
	using System.Threading.Tasks;
	[TestFixture]
	public class FixtureAsync : BugTestCase
	{
		protected override void OnSetUp()
		{
			base.OnSetUp();
			using (ISession s = OpenSession())
			{
				ITransaction t = s.BeginTransaction();
				s.Save(new LogEvent() { Name = "name parameter", Level = "Fatal" });
				s.Save(new LogEvent() { Name = "name parameter", Level = "NonFatal" });
				s.Save(new LogEvent() { Name = "name parameter", Level = "Fatal" });
				t.Commit();
			}
		}

		protected override void OnTearDown()
		{
			base.OnTearDown();
			base.OnSetUp();
			using (ISession s = OpenSession())
			{
				ITransaction t = s.BeginTransaction();
				s.CreateQuery("delete from System.Object").ExecuteUpdate();
				t.Commit();
			}
		}

		[Test]
		public async Task ConditionalAggregateProjectionAsync()
		{
			IProjection isError =
				Projections.Conditional(
					Expression.Eq("Level", "Fatal"),
					Projections.Constant(1),
					Projections.Constant(0));

			using (ISession s = OpenSession())
			{
				IList<object[]> actual =
				await (s.CreateCriteria<LogEvent>()
					.Add(Expression.Eq("Name", "name parameter"))
					.SetProjection(Projections.ProjectionList()
						.Add(Projections.RowCount())
						.Add(Projections.Sum(isError)))
					.ListAsync<object[]>());

				Assert.That(actual.Count, Is.EqualTo(1));
				Assert.That(actual[0][0], Is.EqualTo(3));
				Assert.That(actual[0][1], Is.EqualTo(2));
			}
		}
	}
}
