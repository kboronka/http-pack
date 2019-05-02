using System;
using System.Text;


using HttpPack.Json;
using HttpPack.Fsm;

namespace HttpPack.Server
{
    public class HttpExceptionContent : HttpContent
    {
        public HttpExceptionContent(Exception ex) : base()
        {
            var inner = ExceptionHelper.GetInner(ex);
            var json = new JsonKeyValuePairs();
            json.Add("message", inner.Message);
            json.Add("stackTrace", ExceptionHelper.GetStackTrace(inner));

            var body = json.Stringify();

            this.content = Encoding.UTF8.GetBytes(body);
            this.ContentType = "application/json";
        }
    }
}
