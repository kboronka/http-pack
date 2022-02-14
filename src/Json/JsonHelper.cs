using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace HttpPack
{
    public static class JsonHelper
    {
        public static string ToJson(this object value)
        {
            if (value == null)
            {
                return "null";
            }

            if (value is string)
            {
                return ToJson((object) (string) value);
            }

            if (value is string[])
            {
                return ToJson((object) (string[]) value);
            }

            if (value is string[][])
            {
                return ToJson((object) (string[][]) value);
            }

            if (value is int)
            {
                return ToJson((object) (int) value);
            }

            if (value is int[])
            {
                return ToJson((object) (int[]) value);
            }

            if (value is int[][])
            {
                return ToJson((object) (int[][]) value);
            }

            if (value is long)
            {
                return ToJson((object) (long) value);
            }

            if (value is long[])
            {
                return ToJson((object) (long[]) value);
            }

            if (value is long[][])
            {
                return ToJson((object) (long[][]) value);
            }

            if (value is double)
            {
                return ToJson((object) (double) value);
            }

            if (value is double[])
            {
                return ToJson((object) (double[]) value);
            }

            if (value is double[][])
            {
                return ToJson((object) (double[][]) value);
            }

            if (value is float)
            {
                return ToJson((object) (float) value);
            }

            if (value is float[])
            {
                return ToJson((object) (float[]) value);
            }

            if (value is float[][])
            {
                return ToJson((object) (float[][]) value);
            }

            if (value is DateTime)
            {
                return ToJson((object) (DateTime) value);
            }

            if (value is bool)
            {
                return ToJson((object) (bool) value);
            }

            if (value is bool[])
            {
                return ToJson((object) (bool[]) value);
            }

            if (value is bool[][])
            {
                return ToJson((object) (bool[][]) value);
            }

            if (value is IEnumerable<object>)
            {
                return ToJson((object) (IEnumerable<object>) value);
            }

            if (value is Dictionary<string, object>)
            {
                return ToJson((object) (Dictionary<string, object>) value);
            }

            if (value is IJsonObject)
            {
                return ToJson((object) (IJsonObject) value);
            }

            return "unknown";
        }

        public static string ToJson(this int obj)
        {
            return obj.ToString();
        }

        public static string ToJson(this int[] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this int[][] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this long obj)
        {
            return obj.ToString();
        }

        public static string ToJson(this long[] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this long[][] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this double obj)
        {
            return obj.ToString();
        }

        public static string ToJson(this double[] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this double[][] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this float obj)
        {
            return obj.ToString();
        }

        public static string ToJson(this float[] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this float[][] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this object[] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this object[][] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this string obj)
        {
            var data = obj;

            // escape quotes, and solidus
            data = Regex.Replace(data, @"[\\]", @"\\");
            data = Regex.Replace(data, @"[\""]", @"\""");

            // escape other control-characters
            data = Regex.Replace(data, @"[\n]", @"\n");
            data = Regex.Replace(data, @"[\r]", @"\r");
            data = Regex.Replace(data, @"[\t]", @"\t");
            data = Regex.Replace(data, @"[\b]", @"\b");
            data = Regex.Replace(data, @"[\f]", @"\f");
            data = @"""" + data + @"""";
            return data;
        }

        public static string ToJson(this string[] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this string[][] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static bool IsJson(this string obj)
        {
            if (string.IsNullOrEmpty(obj) || obj.Length < 2)
            {
                return false;
            }

            if (obj[0] == '[' && obj[obj.Length - 1] == ']')
            {
                return true;
            }

            if (obj[0] == '{' && obj[obj.Length - 1] == '}')
            {
                return true;
            }

            return false;
        }

        public static string ToJson(this bool obj)
        {
            return obj ? @"true" : @"false";
        }

        public static string ToJson(this bool[] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this bool[][] objs)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var obj in objs)
            {
                JSON += delimitor;
                JSON += obj.ToJson();
                delimitor = ", ";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this DateTime obj)
        {
            return @"""" + obj.ToString(StringHelper.Iso8601) + @"""";
        }

        public static string ToJson(this IJsonObject obj)
        {
            return obj.KeyValuePairs.Stringify();
        }

        public static string ToJson(this IEnumerable<IJsonObject> obj)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var i in obj)
            {
                JSON += delimitor;
                JSON += i.KeyValuePairs.Stringify();
                delimitor = ",";
            }

            JSON += "]";
            return JSON;
        }

        public static string ToJson(this IEnumerable<object> obj)
        {
            var JSON = "[";
            var delimitor = "";

            foreach (var i in obj)
            {
                JSON += delimitor;
                JSON += i.ToJson();
                delimitor = ",";
            }

            JSON += "]";
            return JSON;
        }

        public static string Stringify(this JsonKeyValuePairs kvp)
        {
            var builder = new JsonBuilder();
            builder.Render(kvp);
            return builder.ToString();
        }

        public static string ToJson(this Dictionary<string, object> obj)
        {
            var JSON = "{";
            var delimitor = "";

            foreach (var key in obj.Keys)
            {
                JSON += delimitor;
                JSON += @"""" + key + @""":" + obj[key].ToJson();
                delimitor = ",";
            }

            JSON += "}";

            return JSON;
        }

        public static JsonKeyValuePairs GetJsonKeyValuePairs(this string json)
        {
            return new JsonKeyValuePairs(json);
        }

        public static JsonArray GetJsonArray(this string json)
        {
            return new JsonArray(json.TrimWhiteSpace());
        }

        public static JsonCollection GetJsonCollection(this string json)
        {
            return new JsonCollection(json);
        }

        public static bool IsObject(string json)
        {
            var startpoint = json.IndexOf(':');

            if (startpoint == -1 && json.Length > 0 && json[0] == '{')
            {
                return true;
            }

            for (var i = startpoint; i < json.Length; i++)
            {
                if (json[i] == '{')
                {
                    return true;
                }

                if (json[i] != '\n' && json[i] != '\r' && json[i] != ' ')
                {
                    return false;
                }
            }

            return false;
        }

        public static object ValueToObject(string value)
        {
            value = value.TrimWhiteSpace();

            if (value.Length == 0)
            {
                return null;
            }

            var firstCharacter = value[0];

            if (firstCharacter == '"')
            {
                var rawValue = value.Substring(1, value.Length - 2);
                return Regex.Unescape(rawValue);
            }

            if (firstCharacter == '{' && IsObject(value))
            {
                return value.GetJsonCollection();
            }

            if (firstCharacter == '{')
            {
                return value.GetJsonKeyValuePairs();
            }

            if (firstCharacter == '[')
            {
                return value.GetJsonArray();
            }

            if (value.IsNumeric())
            {
                if (value.Contains('.'))
                {
                    return double.Parse(value);
                }

                return int.Parse(value);
            }

            if (value == "true")
            {
                return true;
            }

            if (value == "false")
            {
                return false;
            }

            if (value == "null")
            {
                return null;
            }

            // TODO: are timestamps handeled?
            // should we throw an exception here?
            return null;
        }

        public static string BytesToJson(byte[] data)
        {
            // TODO: check for { }
            var json = StringHelper.GetString(data);
            //json = System.Text.Encoding.ASCII.GetString(data);

            // render escaped control characters
            json = Regex.Replace(json, @"([^\\]|^)([\\][n])", m => m.Groups[1].Value + "\n");
            json = Regex.Replace(json, @"([^\\]|^)([\\][r])", m => m.Groups[1].Value + "\r");
            json = Regex.Replace(json, @"([^\\]|^)([\\][t])", m => m.Groups[1].Value + "\t");
            json = Regex.Replace(json, @"([^\\]|^)([\\][b])", m => m.Groups[1].Value + "\b");
            json = Regex.Replace(json, @"([^\\]|^)([\\][f])", m => m.Groups[1].Value + "\f");
            json = Regex.Replace(json, @"([^\\]|^)([\\][""])", m => m.Groups[1].Value + @"""");
            json = Regex.Replace(json, @"([\\][\\])", @"\");

            return json;
        }

        public static int GetJsonValue(this string json, string key, int defaultValue)
        {
            try
            {
                var kvp = json.GetJsonKeyValuePairs();

                if (kvp.ContainsKey(key))
                {
                    return (int) kvp[key];
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static double GetJsonValue(this string json, string key, double defaultValue)
        {
            try
            {
                var kvp = json.GetJsonKeyValuePairs();

                if (kvp.ContainsKey(key))
                {
                    return (double) kvp[key];
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static string GetJsonValue(this string json, string key, string defaultValue)
        {
            try
            {
                var kvp = json.GetJsonKeyValuePairs();

                if (kvp.ContainsKey(key))
                {
                    return (string) kvp[key];
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static bool GetJsonValue(this string json, string key, bool defaultValue)
        {
            try
            {
                var kvp = json.GetJsonKeyValuePairs();

                if (kvp.ContainsKey(key))
                {
                    return (bool) kvp[key];
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static DateTime GetJsonValue(this string json, string key, DateTime defaultValue)
        {
            try
            {
                var kvp = json.GetJsonKeyValuePairs();

                if (kvp.ContainsKey(key))
                {
                    return DateTime.Parse((string) kvp[key], null, DateTimeStyles.RoundtripKind);
                }

                return defaultValue;
            }
            catch
            {
                return defaultValue;
            }
        }

        public static IEnumerable<string> GetJsonStringArray(this string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                throw new ApplicationException("invalid json string array");
            }

            if (json.Length < 2)
            {
                throw new ApplicationException("invalid json string array");
            }

            var level = 0;
            var startFound = false;
            var start = 1;

            for (var i = 0; i < json.Length; i++)
            {
                var x = json[i];

                if (!startFound)
                {
                    if (x == '[')
                    {
                        startFound = true;
                        start = i + 1;
                    }
                }
                else if (x == '{')
                {
                    level++;
                }
                else if (x == '}')
                {
                    level--;
                }
                else if (level == 0 && x == ',')
                {
                    yield return json.Substring(start, i - start);
                    start = i + 1;
                }
                else if (level == 0 && x == ']')
                {
                    yield return json.Substring(start, i - start);
                    break;
                }
            }
        }

        public static JsonKeyValuePairs MergeKeyValuePairs(JsonKeyValuePairs set1, JsonKeyValuePairs set2)
        {
            var result = new JsonKeyValuePairs();
            foreach (var key in set1.Keys)
            {
                result.Add(key, set1[key]);
            }

            foreach (var key in set2.Keys)
            {
                if (set1.ContainsKey(key))
                {
                    if (set1[key].IsA<JsonKeyValuePairs>() && set2[key].IsA<JsonKeyValuePairs>())
                    {
                        var subset1 = (JsonKeyValuePairs) set1[key];
                        var subset2 = (JsonKeyValuePairs) set2[key];
                        result[key] = MergeKeyValuePairs(subset1, subset2);
                    }
                }
                else
                {
                    result.Add(key, set2[key]);
                }
            }

            return result;
        }

        public static JsonKeyValuePairs FlattenJson(JsonKeyValuePairs json)
        {
            return FlattenJson(json, "");
        }

        public static List<string> JsonArrayToCSV(this JsonArray jsonArray)
        {
            var rows = new List<string>();
            if (jsonArray != null)
            {
                var row = string.Empty;
                for (var i = 0; i < jsonArray.Count; i++)
                {
                    if (jsonArray[i].IsA<JsonArray>())
                    {
                        rows.AddRange(JsonArrayToCSV(jsonArray[i] as JsonArray));
                    }
                    else
                    {
                        var value = jsonArray[i].ToString();

                        if (value.Contains(","))
                        {
                            value = "\"" + value + "\"";
                        }

                        row += value + (i != jsonArray.Count - 1 ? "," : string.Empty);
                    }
                }

                if (!string.IsNullOrEmpty(row))
                {
                    rows.Add(row);
                }
            }

            return rows;
        }

        private static JsonKeyValuePairs FlattenJson(JsonKeyValuePairs json, string rootKey)
        {
            var flat = new JsonKeyValuePairs();
            foreach (var key in json.Keys)
            {
                var value = json[key];
                var newKey = rootKey + (string.IsNullOrEmpty(rootKey) ? "" : ".") + key;
                if (value.IsA<JsonKeyValuePairs>())
                {
                    var flattened = FlattenJson((JsonKeyValuePairs) value, newKey);
                    flat = MergeKeyValuePairs(flat, flattened);
                }
                else
                {
                    flat.Add(newKey, value);
                }
            }

            return flat;
        }
    }
}