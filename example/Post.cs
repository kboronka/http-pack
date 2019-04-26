/*
 * Created by SharpDevelop.
 * User: kboronka
 * Date: 4/25/2019
 * Time: 10:36 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

using ApiTools.Json;

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
				var kvp = new JsonKeyValuePairs();
				kvp.Add("title", Title);
				kvp.Add("body", Body);
				kvp.Add("userId", UserID);
				return kvp;
			}
		}
	}
}
