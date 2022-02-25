using HttpPack.Json;
using Xunit;

namespace HttpPack.Tests;

public class UnitTest1
{
    [Theory]
    [InlineData("{\"test\":\"abc\"}", "test", "abc")]
    public void ParseKeyValuePair(string json, string key, object value)
    {
        var kvp = new JsonKeyValuePairs(json);
        Assert.True(kvp.ContainsKey(key));
        Assert.True(kvp.ValidateKey(key));
        Assert.Equal(value, kvp[key]);
    }
}