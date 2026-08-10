using System.Collections.Generic;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnassignedGetOnlyAutoProperty

namespace NHibernate.Test.NHSpecificTest.GH1122
{
	class Person
	{
		private readonly ISet<MusicStyle> _musicStyles = new HashSet<MusicStyle>();
		private readonly ISet<Person> _blacklist = new HashSet<Person>();

		public virtual int Id { get; set; }
		public virtual ISet<MusicStyle> MusicStyles => _musicStyles;
		public virtual ISet<Person> Blacklist => _blacklist;
	}

	class MusicStyle
	{
		public virtual int Id { get; set; }
	}
}
