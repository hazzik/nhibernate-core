namespace NHibernate.Cfg.Loquacious
{
	/// <summary>
	/// Properties of TypeDef configuration.
	/// </summary>
	public class TypeDefConfigurationProperties 
	{
		internal static TypeDefConfigurationProperties Create<T>()
		{
			return new TypeDefConfigurationProperties {Alias = typeof(T).Name};
		}

		public string Alias { get; set; }
		public object Properties { get; set; }
	}
}
