﻿using HttpPack.Json;
using HttpPack.Server;

namespace Example;

[IsPrimaryController]
[IsController]
internal class SampleController
{
    [IsPrimaryAction]
    public static HttpContent JsonContentSample(HttpRequest request)
    {
        var json = new JsonKeyValuePairs
        {
            {"testString", "test"},
            {"testInt", 1234}
        };

        return new HttpJsonContent(json);
    }
}