using System.Collections.Generic;

namespace NHibernate.Test.NHSpecificTest.GH1220
{
	public class Product
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IDictionary<Country, ProductCountryVariation> CountryVariations { get; set; } = new Dictionary<Country, ProductCountryVariation>();
	}

	public class Country
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	public class ProductCountryVariation
	{
		public virtual int Id { get; set; }
		public virtual string LocalName { get; set; }
	}
}
