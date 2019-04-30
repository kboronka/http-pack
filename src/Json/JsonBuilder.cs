using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;

using sar.Tools;
namespace sar.Tools
{
	
}
namespace HttpPack.Json
{
	/// <summary>
	/// Description of JsonBuilder.
	/// </summary>
	public class JsonBuilder
	{
		private readonly StringBuilder builder;
		private readonly NumberFormatInfo numberFormat;
		
		public JsonBuilder()
		{
			builder = new StringBuilder();
			
			numberFormat = new NumberFormatInfo();
			numberFormat.NumberDecimalSeparator = ".";
		}
		
		public JsonBuilder Render(object value)
		{
			if (value == null)
			{
				builder.Append("null");
				return this;
			}
			
			if (value is String)
			{
				return Append((string)value);
			}
			
			if (value is object[] || value is int[] || value is long[] || value is double[] || value is float[] || value is bool[])
			{
				return Append((object[])value);
			}
			
			if (value is object[][] || value is int[][] || value is long[][] || value is double[][] || value is float[][] || value is bool[][])
			{
				return Append((object[][])value);
			}
			
			if (value is int)
			{
				return Append((int)value);
			}
			
			if (value is long)
			{
				return Append((long)value);
			}
			
			if (value is double)
			{
				return Append((double)value);
			}
			
			if (value is float)
			{
				return Append((float)value);
			}
			
			if (value is DateTime)
			{
				return Append((DateTime)value);
			}
			
			if (value is bool)
			{
				return Append((bool)value);
			}
			
			if (value is IEnumerable<object>)
			{
				return Append((IEnumerable<object>)value);
			}
			
			if (value is Dictionary<string, object>)
			{
				return Append((Dictionary<string, object>)value);
			}
			
			if (value is JsonKeyValuePairs)
			{
				return Append((JsonKeyValuePairs)value);
			}
			
			if (value is IJsonObject)
			{
				return Append((IJsonObject)value);
			}
			
			return Append(value.ToString());
		}

		public JsonBuilder Append(string value)
		{
			string data = value;
			
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
			
			builder.Append(data);
			return this;
		}
		
		public JsonBuilder Append(int value)
		{
			builder.Append(value.ToString());
			return this;
		}
		
		public JsonBuilder Append(long value)
		{
			builder.Append(value.ToString());
			return this;
		}
		
		public JsonBuilder Append(double value)
		{
			if (double.IsNaN(value) || double.IsInfinity(value))
			{
				builder.Append("\"" + value.ToString() + "\"");
			}
			else
			{
				builder.Append(value.ToString(numberFormat));
			}
			
			return this;
		}

		public JsonBuilder Append(float value)
		{
			builder.Append(value.ToString(numberFormat));
			return this;
		}
		
		public JsonBuilder Append(DateTime value)
		{
			builder.Append(@"""");
			builder.Append(value.ToString(StringHelper.Iso8601));
			builder.Append(@"""");
			return this;
		}

		public JsonBuilder Append(bool value)
		{
			builder.Append(value ? @"true" : @"false");
			return this;
		}
		
		#region lists and arrays
		
		public JsonBuilder Append(IEnumerable<object> objects)
		{
			builder.Append("[");
			var i = 0;
			foreach (var o in objects)
			{
				if (i++ > 0)
				{
					builder.Append(",");
				}
				
				this.Render(o);
			}
			
			builder.Append("]");
			return this;
		}
		
		public JsonBuilder Append(object[] values)
		{
			builder.Append("[");
			for (int i = 0; i < values.Count(); i++)
			{
				if (i > 0)
				{
					builder.Append(",");
				}
				
				this.Render(values[i]);
			}
			
			builder.Append("]");
			return this;
		}
		
		public JsonBuilder Render(JsonArray objects)
		{
			builder.Append("[");
			var i = 0;
			foreach (var o in objects)
			{
				if (i++ > 0)
				{
					builder.Append(",");
				}
				
				this.Render(o);
			}
			
			builder.Append("]");
			return this;
		}
		
		#endregion
		
		#region key valued pairs
		
		public JsonBuilder Append(Dictionary<string, object> kvp)
		{
			return this.Append(new JsonKeyValuePairs(kvp));
		}
		
		public JsonBuilder Append(IJsonObject obj)
		{
			return this.Append(obj.KeyValuePairs);
		}
		
		public JsonBuilder Append(JsonKeyValuePairs kvp)
		{
			builder.Append("{");
			
			var i = 0;
			foreach (var item in kvp)
			{
				if (i++ > 0)
				{
					builder.Append(",");
				}
				
				this.Append(item.Key);
				builder.Append(":");
				this.Render(item.Value);
			}
			
			builder.Append("}");
			return this;
		}
		
		#endregion
		
		public override string ToString()
		{
			return builder.ToString();
		}
	}
}
