using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using System;

namespace NHibernate.Test.NHSpecificTest.NH3490
{
	public class SomeEntity
	{
		public virtual Guid Id { get; set; }
		public virtual string OneProperty { get; set; }
		public virtual string AnotherProperty { get; set; }
	}

	public class SomeEntityFullMap : ClassMapping<SomeEntity>
	{
		public SomeEntityFullMap()
		{
			Id(x => x.Id, m => m.Generator(Generators.GuidComb));
			Table("SomeEntityTable");
			EntityName("SomeEntityFull");
			Property(x => x.OneProperty);
			Property(x => x.AnotherProperty);
		}
	}

	public class SomeEntityLiteMap : ClassMapping<SomeEntity>
	{
		public SomeEntityLiteMap()
		{
			Id(x => x.Id, m => m.Generator(Generators.GuidComb));
			Table("SomeEntityTable");
			EntityName("SomeEntityLite");
			Property(x => x.OneProperty);
		}
	}

	public class SomeEntityAnotherTableMap : ClassMapping<SomeEntity>
	{
		public SomeEntityAnotherTableMap()
		{
			Id(x => x.Id, m => m.Generator(Generators.GuidComb));
			Table("SomeEntityAnotherTable");
			EntityName("SomeEntityAnotherTable");
			Property(x => x.AnotherProperty);
		}
	}
}
