using System;

namespace NHibernate.Test.NHSpecificTest.GH2330
{
	public abstract class Node
	{
		public virtual int Id { get; set; }
		public virtual bool Deleted { get; set; }
		public virtual string FamilyName { get; set; }
	}

	public class PersonBase : Node
	{
		public virtual string Login { get; set; }
		public override bool Deleted { get; set; }
	}

	public class UserEntityVisit
	{
		public virtual int Id { get; set; }
		public virtual bool Deleted { get; set; }
		public virtual PersonBase PersonBase { get; set; }
	}
}
