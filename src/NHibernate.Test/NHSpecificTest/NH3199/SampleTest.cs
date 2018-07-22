using System.Collections.Generic;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.NH3199
{
    [TestFixture]
    public class SampleTest : BugTestCase
    {
	    protected override void OnSetUp()
	    {
		    using (var session = OpenSession())
		    using (var transaction = session.BeginTransaction())
		    {
			    var entity = new DomainClass
			    {
				    Id = 1,
				    StringFieldList = new List<StringField>
				    {
					    new StringField {Value = "VALUE"},
					    new StringField()
				    }
			    };
			    session.Save(entity);
			    transaction.Commit();
		    }
    }

        protected override void OnTearDown()
        {
            using (var session = OpenSession())
            using (var transaction = session.BeginTransaction())
            {
	            session.Delete("from System.Object");
                transaction.Commit();
            }
        }

	    [Test]
	    public void ObjectWithNullMembersRetrieved()
	    {
		    using (var session = OpenSession())
		    using (session.BeginTransaction())
		    {
			    var entity = session.Get<DomainClass>(1);

			    Assert.That(entity.StringFieldList, Is.Not.Null);
			    Assert.That(entity.StringFieldList.Count, Is.EqualTo(2));
			    Assert.That(entity.StringFieldList[0], Is.Not.Null);
			    Assert.That(entity.StringFieldList[0].Value, Is.Not.Null);
			    Assert.That(entity.StringFieldList[1]?.Value, Is.Null);
		    }
	    }

	    [Test]
	    public void CanDeleteObjectWithNullElementsInCollection()
	    {
		    using (var session = OpenSession())
		    using (session.BeginTransaction())
		    {
			    var entity = session.Get<DomainClass>(1);

			    session.Delete(entity);
		    }
	    }

	    [Test]
	    public void CanDeleteNullElementFromList()
	    {
		    using (var session = OpenSession())
		    using (var transaction = session.BeginTransaction())
		    {
			    var entity = session.Get<DomainClass>(1);
			    Assert.That(entity.StringFieldList.Count, Is.EqualTo(2));

			    entity.StringFieldList.RemoveAt(1);
	            
			    transaction.Commit();
		    }
	    }
	    
	    [Test]
	    public void CanAddItemToTheListWithNullComponent()
	    {
		    using (var session = OpenSession())
		    using (var transaction = session.BeginTransaction())
		    {
			    var entity = session.Get<DomainClass>(1);

			    Assert.That(entity.StringFieldList.Count, Is.EqualTo(2));
			    
			    entity.StringFieldList.Add(
				    new StringField
				    {
					    Value = "Another"
				    });
	            
			    transaction.Commit();
		    }
	    }
    }
}
