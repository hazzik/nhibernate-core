using System;
using System.Collections.Generic;
using System.Text;
using Iesi.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH2379
{
    public class Organisation
    {
        //internal to TGA
        //private int organisationId;
        public virtual Guid OrganisationId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public virtual ISet<ResponsibleLegalPerson> ResponsibleLegalPersons { get; protected set;}

    }

}
