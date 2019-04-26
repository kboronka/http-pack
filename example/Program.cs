using System;
using System.Net;

using ApiTools.Net;
using ApiTools.Json;

namespace example
{
	class Program
	{
		public static void Main(string[] args)
		{
			var client = new RestClient();
			
			var uri = "https://jsonplaceholder.typicode.com/posts/1";
			Console.WriteLine("HTTP GET " + uri);
			var res = client.Fetch(uri);
			
			Console.WriteLine(String.Format("Response Code: {0}", res.Code));
			
			if (res.Code == 200)
			{
				Console.WriteLine("Response:");
				Console.WriteLine(res.Body);
			}
			
			Console.WriteLine();
			Console.WriteLine();
			
			uri = "https://jsonplaceholder.typicode.com/posts";
			Console.WriteLine("HTTP POST " + uri);
			var post = new Post("foo", "bar", 1);
			var req = new JsonKeyValuePairs();
			var auth = "";
			req.Add("body", post);
			
			res = client.Fetch(uri, WebRequestMethods.Http.Post, req, auth);
			
			Console.WriteLine(String.Format("Response Code: {0}", res.Code));
			if (res.Code == 201)	// 201 = Created
			{
				Console.WriteLine("response: " + res.Body);
			}
			else
			{
				Console.WriteLine("err: " + res.Body);
			}
			
			var kvp = new JsonKeyValuePairs(res.Body);
			Console.WriteLine(string.Format("ID: {0}", kvp["id"]));
			
			
			Console.WriteLine();
			Console.WriteLine();
			Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
	}
}