# http-pack
A C# http library which includes a light http server, http client along with json parsing tools, authorization tools, and other useful things.

## Example Server
```C#
using HttpPack;

public static void Main(string[] args)
{
    var server = new HttpServer(4600, @"C:\wwwroot", null);
            
    Console.Write("Press any key to stop http server . . . ");
    Console.ReadKey(true);
}
```

## Example Server Controller
```C#
using HttpPack;

[IsController]
class SampleController
{
    public static HttpContent JsonContentSample(HttpRequest request)
    {
        var json = new JsonKeyValuePairs
        {
            { "my-string", "test" },
            { "my-number", 1234 }
        };

        return new HttpJsonContent(json);
    }
}
```

## Example Client
```C#
using HttpPack;

public static JsonKeyValuePairs GetPost(int id)
{
    var uri = string.Format("https://jsonplaceholder.typicode.com/posts/{0}", id);
    
    var client = new HttpClient<JsonKeyValuePairs>();
    var auth = "";
    var res = client.Get(uri, auth);
    
    if (res.Code == 200)
    {
        return new JsonKeyValuePairs(res.Body);
    }
    
    return null;
}

public static int CreateNewPost(Post post, string auth)
{
    var json = new JsonKeyValuePairs();
    json.Add("post", post);
    
    var uri = "https://jsonplaceholder.typicode.com/posts";
    var client = new HttpClient<JsonKeyValuePairs>();
    var res = client.Post(uri, json, auth);
    
    if (res.Code == 201)    // 201 = Created
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