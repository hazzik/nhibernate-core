using System;

namespace NHibernate.Test.NHSpecificTest.GH1266
{
	class Client
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
	}

	class Purchase
	{
		public virtual int Id { get; set; }
		public virtual Client Client { get; set; }
		public virtual DateTime Date { get; set; }
	}
}
