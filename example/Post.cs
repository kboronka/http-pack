using System;

using HttpPack;

namespace example
{
	/// <summary>
	/// Description of Post.
	/// </summary>
	public class Post : IJsonObject
	{
		public Post(string title, string body, int userID)
		{
			Title = title;
			Body = body;
			UserID = userID;
		}
		
		public string Title { get; private set; }
		public string Body { get; private set; }
		public int UserID { get; private set; }
		
		public JsonKeyValuePairs KeyValuePairs
		{
			get
			{
                var kvp = new JsonKeyValuePairs
                {
                    { "title", Title },
                    { "body", Body },
                    { "userId", UserID }
                };
                return kvp;
			}
		}
	}
}
