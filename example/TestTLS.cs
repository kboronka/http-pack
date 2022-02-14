using System;
using HttpPack.Client;
using HttpPack.Json;

namespace Example;

public static class TestTls
{
    public static bool Tls11()
    {
        const string uri = "https://www.howsmyssl.com/a/check";

        var client = new HttpClient<JsonKeyValuePairs>();
        var res = client.Get(uri, "");

        if (res.Code != 200)
        {
            return false;
        }

        Console.WriteLine("TLS Test -- " + uri);
        Console.WriteLine("  TLS version: " + res.Body["tls_version"]);
        Console.WriteLine("  rating: " + res.Body["rating"]);
        Console.WriteLine();

        return true;
    }
}