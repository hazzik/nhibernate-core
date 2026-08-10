namespace NHibernate.Test.NHSpecificTest.GH1247
{
	class Entity
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual long GeneratedCount { get; set; }
	}
}
