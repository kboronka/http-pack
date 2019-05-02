# http-pack
A C# library of http-client, json, and authorization classes

## Example Server
```C#
public static void Main(string[] args)
{
    var server = new HttpServer(4600, @"C:\wwwroot", null);
            
    Console.Write("Press any key to stop http server . . . ");
    Console.ReadKey(true);
}
```

## Example Server Controller
```C#
[IsPrimaryController]
[IsController]
class SampleController
{
    [IsPrimaryAction]
    public static HttpContent JsonContentSample(HttpRequest request)
    {
        var json = new JsonKeyValuePairs();
        json.Add("testString", "test");
        json.Add("testInt", 1234);

        return new HttpJsonContent(json);
    }
}
```

## Example Client
```C#
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

public static int CreatePost(Post post, string auth)
{
    var req = new JsonKeyValuePairs();
    req.Add("body", post);
    
    var uri = "https://jsonplaceholder.typicode.com/posts";
    var client = new RestClient();
    var res = client.Fetch(uri, WebRequestMethods.Http.Post, req, auth);
    
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


## dev environment
- [SharpDevelop](http://www.icsharpcode.net/OpenSource/SD/Download/Default.aspx#SharpDevelop5x)
- [Microsoft.NET v4.5.2 SDK](https://www.microsoft.com/en-us/download/details.aspx?id=42637)
