using System;
using System.Text;

using HttpPack.Json;
using HttpPack.Fsm;

namespace HttpPack.Server
{
    public class HttpJsonContent : HttpContent
    {
        public HttpJsonContent(JsonKeyValuePairs json) : base()
        {
            var body = json.Stringify();

            this.content = Encoding.UTF8.GetBytes(body);
            this.ContentType = "application/json";
        }
    }
}
