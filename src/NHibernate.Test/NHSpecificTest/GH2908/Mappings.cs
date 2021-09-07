using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace NHibernate.Test.NHSpecificTest.GH2908
{
	public class PersonMapping : ClassMapping<Person>
	{
		public PersonMapping()
		{
			Table("`Person`");

			Id(x => x.Id, map =>
			{
				map.Generator(Generators.Increment);
				map.Column("`Id`");
			});
		}
	}

	public class ProgrammerMapping : SubclassMapping<Programmer>
	{
		public ProgrammerMapping()
		{
			DiscriminatorValue("Programmer");
		}
	}

	public class ManagerMapping : SubclassMapping<Manager>
	{
		public ManagerMapping()
		{
			DiscriminatorValue("Manager");
		}
	}

	public class GroupMapping : ClassMapping<Group>
	{
		public GroupMapping()
		{
			Table("`Group`");

			Id(x => x.Id, map =>
			{
				map.Generator(Generators.Increment);
				map.Column("`Id`");
			});

			ManyToOne(x => x.Leader, map =>
			{
				map.NotNullable(false);
				map.Column("`Person`");
				map.Class(typeof(Person));
				//map.Class(typeof(Programmer));
				//map.Class(typeof(Manager));
			});
		}
	}
}
