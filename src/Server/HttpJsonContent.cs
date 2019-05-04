using System;
using System.Text;

namespace HttpPack
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
