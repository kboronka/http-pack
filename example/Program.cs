using System;
using System.Net;

using HttpPack.Net;
using HttpPack.Json;

namespace example
{
	class Program
	{
		public static void Main(string[] args)
		{
			var postOne = GetPost(1);
			Console.WriteLine(postOne.Stringify());
			
			var newPost = new Post("foo", "bar", 1);
			var postID = CreatePost(newPost, "");
			Console.WriteLine(String.Format("new post id: {0}", postID));
			
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
		
		/// <summary>
		/// HTTP GET example
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public static JsonKeyValuePairs GetPost(int id)
		{
			var uri = string.Format("https://jsonplaceholder.typicode.com/posts/{0}", id);
			
			var client = new RestClient();
			var res = client.Fetch(uri);
			
			if (res.Code == 200)
			{
				return new JsonKeyValuePairs(res.Body);
			}
			
			return null;
		}
		
		/// <summary>
		/// HTTP POST example
		/// </summary>
		/// <param name="post"></param>
		/// <returns></returns>
		public static int CreatePost(Post post, string auth)
		{
			var req = new JsonKeyValuePairs();
			req.Add("body", post);
			
			var uri = "https://jsonplaceholder.typicode.com/posts";
			var client = new RestClient();
			var res = client.Fetch(uri, WebRequestMethods.Http.Post, req, auth);
			
			if (res.Code == 201)	// 201 = Created
			{
				var kvp = new JsonKeyValuePairs(res.Body);
				return (int)kvp["id"];
			}
			else
			{
				return -1;
			}
		}
	}
}