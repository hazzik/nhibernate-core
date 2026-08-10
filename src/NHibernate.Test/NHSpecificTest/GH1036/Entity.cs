namespace NHibernate.Test.NHSpecificTest.GH1036
{
	class Product
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class OrderLine
	{
		public virtual int Id { get; set; }
		public virtual Product Product { get; set; }
	}
}
