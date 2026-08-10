namespace NHibernate.Test.NHSpecificTest.GH1043
{
	public enum EditingLevel
	{
		Editable = 1,
		ReadOnly = 2,
	}

	public abstract class PersistentObject
	{
		public virtual long Id { get; set; }
	}

	public class Contact : PersistentObject
	{
		public virtual string Name { get; set; }
	}

	public abstract class EntityCriticalAttribute : PersistentObject
	{
	}

	public abstract class EntityCriticalAttribute<TEntity, TAttribute> : EntityCriticalAttribute
		where TEntity : class
	{
		public virtual TEntity Entity { get; set; }
		public virtual TAttribute Attribute { get; set; }
	}

	public class ContactEditingLevelAttribute : EntityCriticalAttribute<Contact, EditingLevel>
	{
	}
}
