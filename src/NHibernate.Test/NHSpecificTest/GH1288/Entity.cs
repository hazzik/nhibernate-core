using System.Collections.Generic;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable CollectionNeverUpdated.Global

namespace NHibernate.Test.NHSpecificTest.GH1288
{
	public class Blog
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual IList<Post> Posts { get; set; } = new List<Post>();
	}

	public class Post
	{
		public virtual int Id { get; set; }
		public virtual string Title { get; set; }
		public virtual Blog Blog { get; set; }
		public virtual ISet<Comment> Comments { get; set; } = new HashSet<Comment>();
	}

	public class Comment
	{
		public virtual int Id { get; set; }
		public virtual string Text { get; set; }
		public virtual Post Post { get; set; }
	}
}
