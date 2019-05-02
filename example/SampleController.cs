using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpPack.Json;
using HttpPack.Server;

namespace example
{
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
}
