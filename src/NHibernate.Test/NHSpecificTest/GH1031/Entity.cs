namespace NHibernate.Test.NHSpecificTest.GH1031
{
	class Product
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual Image Image { get; set; }
	}

	class Image
	{
		public virtual string Caption { get; set; }
		public virtual string Content { get; set; }
	}
}
