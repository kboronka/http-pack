using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using HttpPack;

namespace example
{
    [IsPrimaryController]
    [IsController]
    class SampleController
    {
        [IsPrimaryAction]
        public static HttpContent JsonContentSample(HttpRequest request)
        {
            var json = new JsonKeyValuePairs
            {
                { "testString", "test" },
                { "testInt", 1234 }
            };

            return new HttpJsonContent(json);
        }
    }
}
