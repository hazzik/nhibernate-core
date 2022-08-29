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
using log4net;
using NUnit.Framework;

namespace NHibernate.Test.Join
{
	using System.Threading.Tasks;
	[TestFixture]
	public class JoinCompositeKeyTestAsync : TestCase
	{

		protected override string MappingsAssembly
		{
			get { return "NHibernate.Test"; }
		}

		protected override string[] Mappings
		{
			get
			{
				return new string[] {
					"Join.CompositeKey.hbm.xml"
				};
			}
		}

		ISession s;

		protected override void OnSetUp()
		{
			s = OpenSession();

			objectsNeedDeleting.Clear();
		}

		protected override void OnTearDown()
		{
			s.Flush();
			s.Clear();
			try
			{
				// Delete the objects in reverse order because it is
				// less likely to violate foreign key constraints.
				for (int i = objectsNeedDeleting.Count - 1; i >= 0; i--)
				{
					s.Delete(objectsNeedDeleting[i]);
				}
				s.Flush();
			}
			finally
			{
				//t.Commit();
				s.Close();
			}

			s = null;
		}

		private IList objectsNeedDeleting = new ArrayList();

		[Test]
		public async Task SimpleSaveAndRetrieveAsync()
		{
			EmployeeWithCompositeKey emp = new EmployeeWithCompositeKey(1, 100);
			emp.StartDate = DateTime.Today;
			emp.FirstName = "Karl";
			emp.Surname = "Chu";
			emp.OtherNames = "The Yellow Dart";
			emp.Title = "Rock Star";
			objectsNeedDeleting.Add(emp);

			await (s.SaveAsync(emp));
			await (s.FlushAsync());
			s.Clear();

			EmployeePk pk = new EmployeePk(1, 100);
			EmployeeWithCompositeKey retrieved = await (s.GetAsync<EmployeeWithCompositeKey>(pk));

			Assert.IsNotNull(retrieved);
			Assert.AreEqual(emp.StartDate, retrieved.StartDate);
			Assert.AreEqual(emp.FirstName, retrieved.FirstName);
			Assert.AreEqual(emp.Surname, retrieved.Surname);
			Assert.AreEqual(emp.OtherNames, retrieved.OtherNames);
			Assert.AreEqual(emp.Title, retrieved.Title);
		}
	}
}
