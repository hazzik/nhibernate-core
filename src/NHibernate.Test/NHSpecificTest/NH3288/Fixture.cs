using System;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3288
{
	[TestFixture]
	public class Fixture : BugTestCase
	{
		protected static readonly INHibernateLogger log = NHibernateLogger.For(typeof(Fixture));

		protected override void OnSetUp()
		{
			using (var session = OpenSession())
			using (var transaction = session.BeginTransaction())
			{
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
		//When we add a BusinessUnit to BranchLocation and save it, make sure that a StaleDataException is thrown when applicable.
		/*
		 * First user gets a Branch Location
		 * Second user gets the same branch location and saves a BU association to it
		 * First user fetches the original branch and copies all properties from original branch to it and saves the fetched branch.
		 */
		public void TestOneToOneReplaceStaleDataCheckWithSaveFetchedBranchInSeparateSessions()
		{
			var a1Id = Guid.NewGuid();
			var b1Id = Guid.NewGuid();
			var b2Id = Guid.NewGuid();
			var c1Id = Guid.NewGuid();
			var c2Id = Guid.NewGuid();

			//Create the BranchLocation and BusinessUnit
			using (var session = OpenSession())
			using (var txn = session.BeginTransaction())
			{
				var branch = new BranchLocation {Id = a1Id, Name = "Branch1 Name" };
				session.SaveOrUpdate(branch);

				var businessUnit1 = new BusinessUnit {Id = b1Id, Name = "Business Unit1 Name" };
				session.SaveOrUpdate(businessUnit1);

				var businessUnit2 = new BusinessUnit {Id = b2Id, Name = "Business Unit2 Name" };
				session.SaveOrUpdate(businessUnit2);

				txn.Commit();
			}

			log.Debug("Initial set up completed.");

			BranchLocation branchFromUi;
			using (var session = OpenSession())
			using (var txn = session.BeginTransaction())
			{
				log.Debug("First select of branch location");
				branchFromUi = session.Get<BranchLocation>(a1Id);
				Assert.IsNull(branchFromUi.BusinessUnitAssociation, "BusinessUnitAssociation should be null first.");

				txn.Commit();
			}

			//======================
			//Simulate the second user adding BU2 to Branch1.
			using (var session = OpenSession())
			using (var txn = session.BeginTransaction())
			{
				log.Debug("Before the second get of branch location.");
				var branchLoc1 = session.Get<BranchLocation>(a1Id);
				log.Debug("Before the get of Business Unit2.");
				var businessUnit2 = session.Get<BusinessUnit>(b2Id);

				var assoc2 = new BusinessUnitContainsBranchLocation {Id = c1Id};
				assoc2.BranchLocation = branchLoc1;
				assoc2.BusinessUnit = businessUnit2;
				branchLoc1.BusinessUnitAssociation = assoc2;
				//Comment out the line to update the branch name. If the name is updated, the version of BranchLocation is incremented.
				//branchLoc1.Name = "Updated Branch Name";
				log.Debug("Before merge of branch1");
				branchLoc1 = session.Merge(branchLoc1);
				assoc2 = session.Merge(assoc2);

				txn.Commit();
			}

			//======================
			//Simulate the first user saving the original Branch1 with a new association to a different business unit BU1.
			using (var session = OpenSession())
			using (var txn = session.BeginTransaction())
			{
				var businessUnit1 = session.Get<BusinessUnit>(b1Id);
				var assoc = new BusinessUnitContainsBranchLocation {Id = c2Id};
				assoc.BranchLocation = branchFromUi;
				assoc.BusinessUnit = businessUnit1;

				branchFromUi.BusinessUnitAssociation = assoc;
				log.Debug("Before merging fetchedBranchLocation");
				
				Assert.Throws<StaleObjectStateException>(() =>
				{
					branchFromUi = session.Merge(branchFromUi);
					assoc = session.Merge(assoc);
					txn.Commit();
				});
			}
		}
	}
}
