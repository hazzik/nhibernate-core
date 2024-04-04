using System;

namespace NHibernate.Test.NHSpecificTest.GH3500;

class Entity
{
	public virtual Guid Id { get; set; }
	public virtual int? GroupId1 { get; set; }
	public virtual int? GroupId2 { get; set; }
}
