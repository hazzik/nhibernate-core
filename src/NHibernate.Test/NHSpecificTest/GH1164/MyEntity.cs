namespace NHibernate.Test.NHSpecificTest.GH1164
{
	class MyEntity
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }

		public override string ToString()
		{
			return "Custom:" + Name;
		}
	}
}
