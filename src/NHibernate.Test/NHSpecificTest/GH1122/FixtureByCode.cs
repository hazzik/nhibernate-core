using NHibernate.Mapping.ByCode;
using NHibernate.Cfg.MappingSchema;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1122
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Person>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
					rc.Set(
						x => x.MusicStyles,
						m =>
						{
							m.Access(Accessor.Field);
							m.Table("PersonMusicStyle");
						},
						r => r.ManyToMany());
					rc.Set(
						x => x.Blacklist,
						m =>
						{
							m.Access(Accessor.Field);
							m.Table("PersonBlacklist");
						},
						r => r.ManyToMany());
				});

			mapper.Class<MusicStyle>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Native));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		// Two distinct joins (p.MusicStyles and p.Blacklist) given the same alias 'm' must be
		// rejected, the same way a duplicate alias in the root from-clause is rejected. Instead
		// the second join silently reuses/overwrites the alias, and the query runs without error.
		[Test]
		public void JoiningTwoDifferentAssociationsWithSameAliasThrows()
		{
			using var session = OpenSession();

			Assert.That(
				() => session
					.CreateQuery("select distinct p from Person p join p.MusicStyles m left join p.Blacklist m")
					.List(),
				Throws.TypeOf<QueryException>());
		}
	}
}
