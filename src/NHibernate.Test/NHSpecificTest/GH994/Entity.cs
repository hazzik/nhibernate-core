using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH994
{
	class Entity
	{
		private readonly ICollection<ChildEntity> _children = new List<ChildEntity>();
		public virtual int Id { get; set; }
		public virtual ICollection<ChildEntity> Children => _children;
	}

	class ChildEntity
	{
		public virtual int Id { get; set; }
	}
}
