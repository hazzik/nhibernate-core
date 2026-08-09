namespace NHibernate.Test.NHSpecificTest.GH1190
{
	class Animal
	{
		public virtual int Id { get; set; }
		public virtual string Description { get; set; }
	}

	class Human : Animal
	{
		public virtual string NickName { get; set; }
	}
}
