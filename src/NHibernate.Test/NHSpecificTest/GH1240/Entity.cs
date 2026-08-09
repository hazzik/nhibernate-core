namespace NHibernate.Test.NHSpecificTest.GH1240
{
	class Product
	{
		public virtual int Id { get; set; }
		public virtual double Price { get; set; }
	}

	class ProductDto
	{
		public virtual int Id { get; set; }
		public virtual double Price { get; set; }
	}
}
