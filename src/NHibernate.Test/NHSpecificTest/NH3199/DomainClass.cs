using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH3199
{
    public class DomainClass
    {
	    public int Id { get; set; }

	    public IList<StringField> StringFieldList { get; set; }
    }
}