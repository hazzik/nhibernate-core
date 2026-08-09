using System.Collections.Generic;
using NHibernate.Classic;

namespace NHibernate.Test.NHSpecificTest.GH1142
{
	public class Parent : ILifecycle
	{
		private readonly ICollection<Child> _children = new List<Child>();

		public static int ObservedChildrenCountOnLoad = -1;

		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual ICollection<Child> Children => _children;

		public virtual LifecycleVeto OnSave(ISession s)
		{
			return LifecycleVeto.NoVeto;
		}

		public virtual LifecycleVeto OnUpdate(ISession s)
		{
			return LifecycleVeto.NoVeto;
		}

		public virtual LifecycleVeto OnDelete(ISession s)
		{
			return LifecycleVeto.NoVeto;
		}

		public virtual void OnLoad(ISession s, object id)
		{
			// Accessing the children collection here is expected to work: OnLoad is documented
			// to run after the entity (and its eagerly mapped associations) has been initialized.
			ObservedChildrenCountOnLoad = Children.Count;
		}
	}

	public class Child
	{
		public virtual int Id { get; set; }
	}
}
