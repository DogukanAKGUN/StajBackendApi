using System;
namespace StajBackend.Model
{
	public class Post
	{
		public int Id { get; set; }
		public int userId { get; set; }
		public string title { get; set; }
		public string body { get; set; }
	}
}

