using System.Linq;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Linq;
using NHibernate.Mapping.ByCode;
using NUnit.Framework;

namespace NHibernate.Test.NHSpecificTest.GH1288
{
	[TestFixture]
	public class FixtureByCode : TestCaseMappingByCode
	{
		protected override HbmMapping GetMappings()
		{
			var mapper = new ModelMapper();

			mapper.Class<Blog>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Name);
					rc.Bag(
						x => x.Posts,
						m =>
						{
							m.Key(k => k.Column("BlogId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());
				});

			mapper.Class<Post>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Title);
					rc.ManyToOne(x => x.Blog, m => m.Column("BlogId"));
					rc.Set(
						x => x.Comments,
						m =>
						{
							m.Key(k => k.Column("PostId"));
							m.Cascade(Mapping.ByCode.Cascade.All);
							m.Inverse(true);
						},
						r => r.OneToMany());
				});

			mapper.Class<Comment>(
				rc =>
				{
					rc.Id(x => x.Id, m => m.Generator(Generators.Identity));
					rc.Property(x => x.Text);
					rc.ManyToOne(x => x.Post, m => m.Column("PostId"));
				});

			return mapper.CompileMappingForAllExplicitlyAddedEntities();
		}

		protected override void OnSetUp()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var blog = new Blog { Name = "Blog" };

			var post1 = new Post { Title = "Post 1", Blog = blog };
			post1.Comments.Add(new Comment { Text = "Post 1 - Comment 1", Post = post1 });
			post1.Comments.Add(new Comment { Text = "Post 1 - Comment 2", Post = post1 });

			var post2 = new Post { Title = "Post 2", Blog = blog };
			post2.Comments.Add(new Comment { Text = "Post 2 - Comment 1", Post = post2 });
			post2.Comments.Add(new Comment { Text = "Post 2 - Comment 2", Post = post2 });

			blog.Posts.Add(post1);
			blog.Posts.Add(post2);

			session.Save(blog);

			transaction.Commit();
		}

		protected override void OnTearDown()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			session.CreateQuery("delete from Comment").ExecuteUpdate();
			session.CreateQuery("delete from Post").ExecuteUpdate();
			session.CreateQuery("delete from Blog").ExecuteUpdate();

			transaction.Commit();
		}

		[Test]
		public void DeepLoadOfRootWithBagOfDetailsContainingCollectionOfDeeperEntitiesReturnsCorrectCount()
		{
			using var session = OpenSession();
			using var transaction = session.BeginTransaction();

			var blog = session
				.Query<Blog>()
				.FetchMany(b => b.Posts)
				.ThenFetchMany(p => p.Comments)
				.ToList()
				.Single();

			Assert.That(blog.Posts.Count, Is.EqualTo(2), "Wrong number of Post entities under Blog");

			foreach (var post in blog.Posts)
			{
				Assert.That(post.Comments.Count, Is.EqualTo(2), $"Wrong number of Comment entities under Post '{post.Title}'");
			}

			transaction.Commit();
		}
	}
}
