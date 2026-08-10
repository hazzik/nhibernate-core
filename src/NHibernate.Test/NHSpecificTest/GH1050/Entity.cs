namespace NHibernate.Test.NHSpecificTest.GH1050
{
	class School
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Janitor Janitor { get; set; }
	}

	class Janitor
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class SchoolDto
	{
		public string Name { get; set; }
		public JanitorDto Janitor { get; set; }
	}

	class JanitorDto
	{
		public string Name { get; set; }
	}
}
