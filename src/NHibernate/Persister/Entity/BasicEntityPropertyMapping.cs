using NHibernate.Type;

namespace NHibernate.Persister.Entity
{
	public class BasicEntityPropertyMapping : AbstractPropertyMapping
	{
		private readonly AbstractEntityPersister persister;

		public BasicEntityPropertyMapping(AbstractEntityPersister persister)
		{
			this.persister = persister;
		}

		public override string[] IdentifierColumnNames
		{
			get { return persister.IdentifierColumnNames; }
		}

		protected override string EntityName
		{
			get { return persister.EntityName; }
		}

		public override IType Type
		{
			get { return persister.Type; }
		}

		public override string[] ToColumns(string alias, string propertyName)
		{
			var tableNumber = persister.GetSubclassPropertyTableNumber(persister.GetPropertyIndex(propertyName));
			return base.ToColumns(
				persister.GenerateTableAlias(alias, tableNumber),
				propertyName);
		}
	}
}
