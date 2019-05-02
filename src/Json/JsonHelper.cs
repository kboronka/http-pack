/* Copyright (C) 2018 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace HttpPack.Json
{
	public static class JsonHelper
	{
		public static string ToJson(this object value)
		{
			if (value == null)
			{
				return "null";
			}
			else if (value is String)
			{
				return ((string)value).ToJson();
			}
			else if (value is string[])
			{
				return ((string[])value).ToJson();
			}
			else if (value is string[][])
			{
				return ((string[][])value).ToJson();
			}
			else if (value is int)
			{
				return ((int)value).ToJson();
			}
			else if (value is int[])
			{
				return ((int[])value).ToJson();
			}
			else if (value is int[][])
			{
				return ((int[][])value).ToJson();
			}
			else if (value is long)
			{
				return ((long)value).ToJson();
			}
			else if (value is long[])
			{
				return ((long[])value).ToJson();
			}
			else if (value is long[][])
			{
				return ((long[][])value).ToJson();
			}
			else if (value is double)
			{
				return ((double)value).ToJson();
			}
			else if (value is double[])
			{
				return ((double[])value).ToJson();
			}
			else if (value is double[][])
			{
				return ((double[][])value).ToJson();
			}
			else if (value is float)
			{
				return ((float)value).ToJson();
			}
			else if (value is float[])
			{
				return ((float[])value).ToJson();
			}
			else if (value is float[][])
			{
				return ((float[][])value).ToJson();
			}
			else if (value is DateTime)
			{
				return ((DateTime)value).ToJson();
			}
			else if (value is bool)
			{
				return ((bool)value).ToJson();
			}
			else if (value is bool[])
			{
				return ((bool[])value).ToJson();
			}
			else if (value is bool[][])
			{
				return ((bool[][])value).ToJson();
			}
			else if (value is IEnumerable<object>)
			{
				return ((IEnumerable<object>)value).ToJson();
			}
			else if (value is Dictionary<string, object>)
			{
				return ((Dictionary<string, object>)value).ToJson();
			}
			else if (value is IJsonObject)
			{
				return ((IJsonObject)value).ToJson();
			}

			return "unknown";
		}

		public static string ToJson(this int obj)
		{
			return obj.ToString();
		}

		public static string ToJson(this int[] objs)
		{
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string data = obj;
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			if (String.IsNullOrEmpty(obj) || obj.Length < 2)
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
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
			string JSON = "[";
			string delimitor = "";
			
			foreach (IJsonObject i in obj)
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
			string JSON = "[";
			string delimitor = "";
			
			foreach (object i in obj)
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
			string JSON = "{";
			string delimitor = "";
			
			foreach (string key in obj.Keys)
			{
				JSON += delimitor;
				JSON += @"""" + key + @""":" + obj[key].ToJson() ;
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

			if (startpoint == -1 && json.Length > 0 && json[0] == '{') return true;
			
			for (var i = startpoint; i < json.Length; i++)
			{
				if (json[i] == '{')
				{
					return true;
				}
				else if (json[i] != '\n' && json[i] != '\r' && json[i] != ' ')
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
				return TrimJsonString(value);
			}
			else if (firstCharacter == '{' && IsObject(value))
			{
				return value.GetJsonCollection();
			}
			else if (firstCharacter == '{')
			{
				return value.GetJsonKeyValuePairs();
			}
			else if (firstCharacter == '[')
			{
				return value.GetJsonArray();
			}
			else if (value.IsNumeric())
			{
				if (value.Contains('.'))
				{
					return double.Parse(value);
				}
				
				return int.Parse(value);
			}
			else if (value == "true")
			{
				return true;
			}
			else if (value == "false")
			{
				return false;
			}
			else if (value == "null")
			{
				return null;
			}
			else
			{
				// TODO: are timestamps handeled?
				// should we throw an exception here?
				return null;
			}
		}
		
		public static string TrimJsonString(string value)
		{
			return value.Substring(1, value.Length - 2);
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
					return (int)kvp[key];
				}
				else
				{
					return defaultValue;
				}
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
					return (double)kvp[key];
				}
				else
				{
					return defaultValue;
				}
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
					return (string)kvp[key];
				}
				else
				{
					return defaultValue;
				}
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
					return (bool)kvp[key];
				}
				else
				{
					return defaultValue;
				}
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
					return DateTime.Parse((string)kvp[key], null, System.Globalization.DateTimeStyles.RoundtripKind);
				}
				else
				{
					return defaultValue;
				}
			}
			catch
			{
				return defaultValue;
			}
		}
		
		public static IEnumerable<string> GetJsonStringArray(this string json)
		{
			if (String.IsNullOrEmpty(json))
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
			
			for (int i = 0; i < json.Length; i++)
			{
				char x = json[i];
				
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
					level ++;
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
						var subset1 = (JsonKeyValuePairs)set1[key];
						var subset2 = (JsonKeyValuePairs)set2[key];
						result[key] = MergeKeyValuePairs(subset1, subset2);
					}
					else
					{
						// duplicate
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
				string row = string.Empty;
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
							
						row += value + (i != (jsonArray.Count - 1) ? "," : string.Empty);
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
					var flattened = FlattenJson((JsonKeyValuePairs)value, newKey);
					flat = JsonHelper.MergeKeyValuePairs(flat, flattened);
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
