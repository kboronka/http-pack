using System.Text;
using HttpPack.Json;

namespace HttpPack.Server;

public class HttpJsonContent : HttpContent
{
    public HttpJsonContent(JsonKeyValuePairs json)
    {
        var body = json.Stringify();

        content = Encoding.UTF8.GetBytes(body);
        ContentType = "application/json";
    }
}