using System;
using System.Text;
using HttpPack.Json;
using HttpPack.Utils;

namespace HttpPack.Server;

public class HttpExceptionContent : HttpContent
{
    public HttpExceptionContent(Exception ex)
    {
        var inner = ExceptionHelper.GetInner(ex);
        var json = new JsonKeyValuePairs
        {
            {"message", inner.Message},
            {"stackTrace", ExceptionHelper.GetStackTrace(inner)}
        };

        var body = json.Stringify();

        content = Encoding.UTF8.GetBytes(body);
        ContentType = "application/json";
    }
}