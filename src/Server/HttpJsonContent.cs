using System.Text;

namespace HttpPack
{
    public class HttpJsonContent : HttpContent
    {
        public HttpJsonContent(JsonKeyValuePairs json)
        {
            var body = json.Stringify();

            content = Encoding.UTF8.GetBytes(body);
            ContentType = "application/json";
        }
    }
}