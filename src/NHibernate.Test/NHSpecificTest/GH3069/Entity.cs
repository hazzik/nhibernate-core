namespace NHibernate.Test.NHSpecificTest.GH3069
{
	class Card
	{
		public virtual int Id { get; set; }
		public virtual string CardNo { get; set; }
		public virtual int Dci { get; set; }
	}

	class CardResponse
	{
		public virtual bool IsLimitExceeded { get; set; }
	}
}
