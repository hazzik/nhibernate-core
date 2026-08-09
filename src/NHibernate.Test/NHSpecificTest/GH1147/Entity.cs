namespace NHibernate.Test.NHSpecificTest.GH1147
{
	class SrcObj
	{
		public virtual int Id { get; set; }
		public virtual string Member { get; set; }
	}

	class DestObj
	{
		public virtual int Id { get; set; }
		public virtual string Member { get; set; }
	}
}
