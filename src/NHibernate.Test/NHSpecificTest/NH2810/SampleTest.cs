using System.Collections.Generic;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH2810
{
	[TestFixture]
	public class SampleTest : BugTestCase
	{
		private object _b1Id;

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			{
				var b_1 = new ClassB();
				b_1.Text = "Test ClassB 1";
				_b1Id = session.Save(b_1);
				session.Flush();


				var a_1 = new ClassA();
				a_1.Text = "Test ClassA 1";
				a_1.B_ID = b_1.B_ID;
				session.Save(a_1);
				var a_2 = new ClassA();
				a_2.Text = "Test ClassA 2";
				a_2.B_ID = b_1.B_ID;
				session.Save(a_2);
				var a_3 = new ClassA();
				a_3.Text = "Test ClassA 3";
				a_3.B_ID = b_1.B_ID;
				session.Save(a_3);

				session.Flush();
			}
		}

		protected override void OnTearDown()
		{
			using (var session = OpenSession())
			{
				session.CreateQuery("delete from ClassA").ExecuteUpdate();
				session.CreateQuery("delete from ClassB").ExecuteUpdate();
				session.Flush();
			}
		}

		private ClassB create_TestClass_with_pure_BO()
		{
			var BO_ClassB = new ClassB();
			BO_ClassB.classA_list = new List<ClassA>();
			BO_ClassB.classA_list.Add(new ClassA {A_ID = 1, Text = "A_1"});
			BO_ClassB.classA_list.Add(new ClassA {A_ID = 2, Text = "A_2"});
			BO_ClassB.classA_list.Add(new ClassA {A_ID = 3, Text = "A_3"});

			return BO_ClassB;
		}

		[Test]
		public void Can_NH_IListFindIndexOfObject()
		{
			using (var session = OpenSession())
			{
				var result = session.Get<ClassB>(_b1Id);
				Assert.AreEqual(3, result.classA_list.Count, "NHibernate-Ilist: Not all objects loaded");


				var NH_objToFind = result.classA_list[1]; //lets take second one (with index 1)
				var NH_indx = result.classA_list.IndexOf(NH_objToFind);

				Assert.AreEqual(1, NH_indx, "NHibernate-Ilist: object not found !");
			}
		}

		[Test]
		public void Can_NH_IList_RemoveObject()
		{
			using (var session = OpenSession())
			{
				var result = session.Get<ClassB>(_b1Id);
				Assert.AreEqual(3, result.classA_list.Count, "NHibernate-Ilist: Not all objects loaded");

				var NH_objToDelete = result.classA_list[1]; //lets take second one (with index 1)
				Assert.IsNotNull(NH_objToDelete, "NHibernate-Ilist: object not found !");

				result.classA_list.Remove(NH_objToDelete);

				Assert.AreEqual(2, result.classA_list.Count, "NHibernate-Ilist: object was not removed from list");
			}
		}



		//following test are used to check functions in system lists
		[Test]
		public void Can_SystemListFindIndexOfObject()
		{
			var BO_ClassB = create_TestClass_with_pure_BO();
			var BO_objToFind = BO_ClassB.classA_list[1];
			var BO_indx = BO_ClassB.classA_list.IndexOf(BO_objToFind);

			Assert.AreEqual(1, BO_indx, "System-List: object not found !");
		}

		[Test]
		public void Can_SystemList_RemoveObject()
		{
			var result = create_TestClass_with_pure_BO();
			Assert.AreEqual(3, result.classA_list.Count, "System-Ilist: Not all objects");

			var BO_objToDelete = result.classA_list[1];
			Assert.IsNotNull(BO_objToDelete, "System-Ilist: object not found !");
			result.classA_list.Remove(BO_objToDelete);

			Assert.AreEqual(2, result.classA_list.Count, "System-list: object was not removed from list");
		}
	}
}
