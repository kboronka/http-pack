# api-tools
A C# library of http-client, json, and authorization classes

## Example
```C#
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
```


## dev environment
- [SharpDevelop](http://www.icsharpcode.net/OpenSource/SD/Download/Default.aspx#SharpDevelop5x)
- [Microsoft.NET v4.5.2 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=42637)
