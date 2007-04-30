using System;
using System.Collections;

using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH387
{
	[TestFixture]
	public class Fixture : BugTestCase
    {
        #region Fields
        private static readonly string UseIdentifierRollback = "true";
        #endregion

        #region Overridden Members
        protected override ISession OpenSession()
        {
            lastOpenedSession = sessions.OpenSession();
            return lastOpenedSession;
        }

		public override string BugNumber
		{
			get { return "NH387"; }
		}

        protected override void Configure(NHibernate.Cfg.Configuration cfg)
        {
            // Set the identifier flag 
            cfg.Properties[NHibernate.Cfg.Environment.UseIdentifierRollback] = UseIdentifierRollback;
            base.Configure(cfg);
        }
        #endregion

        #region Non transactional tests
        [Test]
        public void FlushingInsertRollback()
        {

            using (ISession s = OpenSession())
            {
                _Parent p = new _Parent();
                s.Save(p);
                Assert.AreNotEqual(0, p._Id, "Wrong identifier value");
                // session should not be dirty.
                Assert.IsFalse(s.IsDirty());

                s.FlushMode = FlushMode.Auto;
                s.Delete(p);
                s.Flush();

                Assert.AreEqual(0, p._Id, "Identifier value was not reset");
                // session should not be dirty.
                Assert.IsFalse(s.IsDirty());
            }
        }

        [Test]
        public void FlushingDeleteRollback()
        {
            using (ISession s = OpenSession())
            {
                Assert.IsTrue(s.SessionFactory.Settings.IsIdentifierRollbackEnabled, "Identifier rollback not enabled");
                _Parent p = new _Parent();
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                    tx.Commit();
                }
                // session should not be dirty.
                Assert.IsFalse(s.IsDirty());

                s.FlushMode = FlushMode.Auto;
                s.Delete(p);
                s.Flush();

                Assert.AreEqual(0, p._Id, "Identifier value was not reset after delete");

                // session should not be dirty.
                Assert.IsFalse(s.IsDirty());
            }
        }

        [Test]
        public void FlushingCascadingRollback()
        {
            _Parent p = new _Parent();
            p._Children = new ArrayList();
            p._Children.Add(new _Child());
            p._Children.Add(new _Child());

            using (ISession s = OpenSession())
            {
                s.Save(p);
                s.Save(p._Children[0]);
                s.Save(p._Children[1]);
                s.Flush();

                Assert.AreNotEqual(0, p._Id);
                Assert.AreNotEqual(0, ((_Child)p._Children[0])._Id);
                Assert.AreNotEqual(0, ((_Child)p._Children[1])._Id);

                _Child c = (_Child) p._Children[1];
                p._Children.Remove(c);
                s.Delete(c);
                s.Flush();

                Assert.AreEqual(0, c._Id);

                s.Delete(p);
                //s.CreateQuery("from System.Object o").List();
                s.Flush();
            }
        }

        [Test]
        public void AssignedIdentifierRollback()
        {
            using (ISession s = OpenSession())
            {
                Assert.IsTrue(s.SessionFactory.Settings.IsIdentifierRollbackEnabled, "Identifier rollback not enabled");
                _AssignedIdObject p = new _AssignedIdObject();
                p._Id = 123;
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    tx.Commit();
                }
                // session should not be dirty.
                Assert.IsFalse(s.IsDirty());

                s.FlushMode = FlushMode.Auto;
                s.Delete(p);
                s.Flush();

                Assert.AreNotEqual(0, p._Id, "Identifier value was reset after delete");

                // session should not be dirty.
                Assert.IsFalse(s.IsDirty());
            }
        }
        #endregion

        #region Transactional
        [Test]
        public void TransactedInsertCommit()
        {
            using (ISession s = OpenSession())
            {
                _Parent p = new _Parent();
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                    tx.Commit();
                }
                Assert.AreNotEqual(0, p._Id, "Identifier value was wrong after rollback");
                s.Delete(p);
                s.Flush();
            }
        }

        [Test]
        public void TransactedInsertForgotCommit()
        {
            using (ISession s = OpenSession())
            {
                _Parent p = new _Parent();
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                }
                Assert.AreEqual(0, p._Id, "Identifier value was wrong after rollback");
            }
        }

        [Test]
        public void TransactedInsertRollback()
        {

            using (ISession s = OpenSession())
            {
                _Parent p = new _Parent();
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                    tx.Rollback();
                    Assert.AreEqual(0, p._Id, "Identifier value was wrong after rollback");
                }
            }
        }

        [Test]
        public void TransactedInsertWithException()
        {
            // create a unique exception text, since we want to
            // make sure that the caught exception is the one we threw.
            string exceptionText = Guid.NewGuid().ToString();

            _Parent p = new _Parent();
            try
            {
                using (ISession s = OpenSession())
                {
                    using (ITransaction tx = s.BeginTransaction())
                    {
                        s.Save(p);
                        Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                        throw new Exception(exceptionText);
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, exceptionText);
                Assert.AreEqual(0, p._Id, "Identifier value was wrong after an exception was thrown");
            }
        }

        [Test]
        public void TransactedUpdateRollback()
        {

            _Parent p = new _Parent();
            using (ISession s = OpenSession())
            {
                //Assert.IsTrue(s.SessionFactory.Settings.IsIdentifierRollbackEnabled, "Identifier rollback not enabled");
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                    tx.Commit();
                }
            }
            using (ISession s = OpenSession())
            {
                p.Text = "Hello world";
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Update(p);
                    tx.Commit();
                    //tx.Rollback();
                }
                Assert.AreNotEqual(0, p._Id, "Identifier value was wrong after rollback");

                s.Delete(p);
                s.Flush();
            }
        }

        [Test]
        public void TransactedDeleteCommit()
        {

            _Parent p = new _Parent();
            using (ISession s = OpenSession())
            {
                //Assert.IsTrue(s.SessionFactory.Settings.IsIdentifierRollbackEnabled, "Identifier rollback not enabled");
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                    tx.Commit();
                }
            }
            using (ISession s = OpenSession())
            {
                p.Text = "Hello world";
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Delete(p);
                    tx.Commit();
                    //tx.Rollback();
                }
                Assert.AreEqual(0, p._Id, "Identifier value was wrong after rollback");
            }
        }

        [Test]
        public void TransactedDeleteWithException()
        {
            // create a unique exception text, since we want to
            // make sure that the caught exception is the one we threw.
            string exceptionText = Guid.NewGuid().ToString();

            _Parent p = new _Parent();

            using (ISession s = OpenSession())
            {
                //Assert.IsTrue(s.SessionFactory.Settings.IsIdentifierRollbackEnabled, "Identifier rollback not enabled");
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    Assert.AreNotEqual(0, p._Id, "Insert statement failed");
                    tx.Commit();
                }
            }

            try
            {
                using (ISession s = OpenSession())
                {
                    using (ITransaction tx = s.BeginTransaction())
                    {
                        s.Delete(p);
                        throw new Exception(exceptionText);
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.AreEqual(ex.Message, exceptionText);
                Assert.AreNotEqual(0, p._Id, "Identifier value was wrong after an exception was thrown");
            }
            finally
            {
                using (ISession s = OpenSession())
                {
                    s.Delete(p);
                    s.Flush();
                }
            }
        }

        [Test]
        public void TransactedCascadingRollback()
        {
            using (ISession s = OpenSession())
            {
                _Parent p = new _Parent();
                p._Children = new ArrayList();
                p._Children.Add(new _Child());
                p._Children.Add(new _Child());
                using (ITransaction tx = s.BeginTransaction())
                {
                    s.Save(p);
                    s.Save(p._Children[0]);
                    s.Save(p._Children[1]);

                    Assert.AreNotEqual(0, p._Id);
                    Assert.AreNotEqual(0, ((_Child) p._Children[0])._Id);
                    Assert.AreNotEqual(0, ((_Child)p._Children[1])._Id);
                    tx.Rollback();
                }
                Assert.AreEqual(0, p._Id);
                Assert.AreEqual(0, ((_Child)p._Children[0])._Id);
                Assert.AreEqual(0, ((_Child)p._Children[1])._Id);
            }
        }
        #endregion
    }
}
