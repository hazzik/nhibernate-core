using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Impl;
using NHibernate.Mapping.ByCode.Impl.CustomizersImpl;

namespace NHibernate.Test.NHSpecificTest.GH1043
{
	// Reproduces, as closely as possible, the reporter's own generic mapping-by-code helper
	// classes from NH-3809 / GitHub issue #1043: a generic subclass-customizer base class that
	// registers the property mapping through a method inherited from a generic type parameter.
	public abstract class EntityCriticalAttributeMapping<TEntityCriticalAttribute, TEntity, TAttribute>
		: SubclassCustomizer<TEntityCriticalAttribute>
		where TEntityCriticalAttribute : EntityCriticalAttribute<TEntity, TAttribute>
		where TEntity : class
	{
		protected EntityCriticalAttributeMapping()
			: base(GetExplicitDeclarationsHolderWithSubClass(), GetCustomizersHolderWithSubClass())
		{
			DiscriminatorValue(typeof(TEntityCriticalAttribute).FullName);

			ManyToOne(x => x.Entity, m =>
			{
				m.Column("entity_id");
				m.Fetch(FetchKind.Join);
				m.Lazy(LazyRelation.NoLazy);
			});
		}

		// this method is called from the concrete mapping class constructor
		protected void EnumAsStringAttribute()
		{
			// per the original report, this customization was silently never applied
			Property(x => x.Attribute, m =>
			{
				m.Column("attribute_value_string");
				m.Length(500);
				m.Type<NHibernate.Type.EnumStringType<TAttribute>>();
			});
		}

		private static IModelExplicitDeclarationsHolder GetExplicitDeclarationsHolderWithSubClass()
		{
			var explicitDeclarationsHolder = new ExplicitDeclarationsHolder();
			explicitDeclarationsHolder.AddAsTablePerClassHierarchyEntity(typeof(EntityCriticalAttribute<TEntity, TAttribute>));
			return explicitDeclarationsHolder;
		}

		private static ICustomizersHolder GetCustomizersHolderWithSubClass()
		{
			var customizersHolder = new CustomizersHolder();
			customizersHolder.AddCustomizer(typeof(EntityCriticalAttribute<TEntity, TAttribute>), (ISubclassMapper m) => { });
			return customizersHolder;
		}
	}

	public class ContactEditingLevelAttributeMapping
		: EntityCriticalAttributeMapping<ContactEditingLevelAttribute, Contact, EditingLevel>
	{
		public ContactEditingLevelAttributeMapping()
		{
			EnumAsStringAttribute();
		}
	}
}
