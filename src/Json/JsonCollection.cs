using System;

namespace HttpPack.Json;

/// <summary>
///     Description of JsonCollection.
/// </summary>
public class JsonCollection : JsonKeyValuePairs
{
    public JsonCollection()
    {
    }

    public JsonCollection(string json)
    {
        var depth = 0;

        var valueStart = -1;
        var valueEnd = -1;
        var value = "";

        var keyStart = -1;
        var keyEnd = -1;
        var key = "";

        for (var i = 0; i < json.Length; i++)
        {
            var c = json[i];

            if (c == '{')
            {
                depth++;
            }
            else if (c == ':' && depth == 1)
            {
                valueStart = i + 1;
            }
            else if (c == '}')
            {
                depth--;

                if (depth == 1)
                {
                    valueEnd = i;

                    try
                    {
                        key = json.Substring(keyStart, keyEnd - keyStart + 1).TrimWhiteSpace();
                        value = json.Substring(valueStart, valueEnd - valueStart + 1).TrimWhiteSpace();
                        Add(key, JsonHelper.ValueToObject(value));
                        keyStart = -1;
                        keyEnd = -1;
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            else if (c == '"' && depth == 1 && keyStart == -1)
            {
                keyStart = i + 1;
            }
            else if (c == '"' && depth == 1 && keyEnd == -1)
            {
                keyEnd = i - 1;
            }
        }
    }
}