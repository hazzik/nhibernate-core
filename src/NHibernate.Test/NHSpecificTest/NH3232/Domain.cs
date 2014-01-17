using System;

namespace NHibernate.Test.NHSpecificTest.NH3232
{
	public class Page
	{
		public virtual string CreatedBy { get; set; }
		public virtual DateTime CreatedOn { get; set; }
		public virtual int Id { get; set; }
		public virtual bool IsLocked { get; set; }
		public virtual string ModifiedBy { get; set; }
		public virtual DateTime ModifiedOn { get; set; }
		public virtual string Tags { get; set; }
		public virtual string Title { get; set; }
	}

	public class PageContent
	{
		public virtual Guid Id { get; set; }
		public virtual string EditedBy { get; set; }
		public virtual DateTime EditedOn { get; set; }
		public virtual int VersionNumber { get; set; }
		public virtual string Text { get; set; }
		public virtual Page Page { get; set; }
	}
}
