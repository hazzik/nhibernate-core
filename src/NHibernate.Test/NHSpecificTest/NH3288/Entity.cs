using System;
using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.NH3288
{
	public class BranchLocation 
	{
		public virtual Guid Id { get; set; }
		public virtual int Version { get; set; }
		public virtual string Name { get; set; }
		public virtual BusinessUnitContainsBranchLocation BusinessUnitAssociation { get; set; }
	}

	public class BusinessUnit
	{
		public virtual Guid Id { get; set; }
		public virtual int Version { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<BusinessUnitContainsBranchLocation> BranchLocations { get; set; }
	}

	public class BusinessUnitContainsBranchLocation 
	{
		public virtual Guid Id { get; set; }
		public virtual int Version { get; set; }
		public virtual string Name { get; set; }

		public virtual BranchLocation BranchLocation { get; set; }
		public virtual BusinessUnit BusinessUnit { get; set; }

	}

}
