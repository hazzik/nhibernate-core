using System;

namespace NHibernate.Test.NHSpecificTest.GH1171
{
	public enum ClassificationType
	{
		Unknown,
		Company,
		Other
	}

	public class Classification
	{
		public virtual int Id { get; set; }
		public virtual ClassificationType Type { get; set; }
	}

	public class SpecialTemplate
	{
		private Classification _templateType;

		public virtual int Id { get; set; }

		public virtual Classification TemplateType
		{
			get => _templateType;
			set
			{
				if (value != null && value.Type != ClassificationType.Company)
				{
					throw new ArgumentException("TemplateType must be a Company classification", nameof(value));
				}

				_templateType = value;
			}
		}
	}

	public class TemplateGroup
	{
		public virtual int Id { get; set; }
		public virtual SpecialTemplate MySpecialTemplate { get; set; }
	}
}
