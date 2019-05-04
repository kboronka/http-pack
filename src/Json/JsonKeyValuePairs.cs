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

namespace HttpPack
{
	/// <summary>
	/// Dictionary extension object.
	/// </summary>
	public class JsonKeyValuePairs : Dictionary<string, object>
	{
		public JsonKeyValuePairs()
			: base()
		{
			
		}
		
		public JsonKeyValuePairs(Dictionary<string, object> dictionary)
		{
			foreach (string key in dictionary.Keys)
			{
				base.Add(key, dictionary[key]);
			}
		}
		
		public JsonKeyValuePairs(string json)
			: base()
		{
			var depth = 0;
			var stringDepth = 0;
			
			var keyStart = -1;
			var keyEnd = -1;
			var key = "";
			
			var valueStart = -1;
			var valueEnd = -1;
			var value = "";
			
			for (var i = 0; i < json.Length; i++)
			{
				var c = json[i];
				
				if (c == '{' && stringDepth == 0)
				{
					depth++;
				}
				else if (c == '[' && depth > 0 && stringDepth == 0)
				{
					depth++;
				}
				else if (c == ']' && depth > 0 && stringDepth == 0)
				{
					depth--;
				}
				else if (c == '"' && depth > 0 && stringDepth == 0)
				{
					stringDepth++;
					
					if (keyStart == -1 && depth == 1)
					{
						// start of key
						keyStart = i;
					}
				}
				else if (c == '"' && depth > 0 && stringDepth == 1)
				{
					stringDepth--;
					
					if (keyEnd == -1 && depth == 1)
					{
						// end of a key
						keyEnd = i;
						key = JsonHelper.TrimJsonString(json.Substring(keyStart, keyEnd - keyStart + 1));
					}
				}
				else if (c == ':' && stringDepth == 0)
				{
					if (valueStart == -1 && depth == 1)
					{
						// start of value
						valueStart = i + 1;
					}
				}
				else if (c == ',' && stringDepth == 0)
				{
					if (valueEnd == -1 && depth == 1)
					{
						// end of value
						valueEnd = i - 1;
						value = json.Substring(valueStart, valueEnd - valueStart + 1);
						
						if (!this.ContainsKey(key))
						{
							this.Add(key, JsonHelper.ValueToObject(value));
						}
						
						// prep for next key
						keyStart = -1;
						keyEnd = -1;
						key = "";
						valueStart = -1;
						valueEnd = -1;
						value = "";
					}
				}
				else if (c == '}' && stringDepth == 0)
				{
					if (depth == 1)
					{
						valueEnd = i - 1;
						value = json.Substring(valueStart, valueEnd - valueStart + 1);
						
						this.Add(key, JsonHelper.ValueToObject(value));
					}
					
					depth--;
				}
			}
			
			if (stringDepth != 0 && depth != 0)
			{
				throw new ApplicationException("Invalid json string");
			}
		}
		
		new public void Add(string key, object value)
		{
			if (!base.ContainsKey(key))
			{
				base.Add(key, value);
			}
		}
		
		public void AddRange(JsonKeyValuePairs kvp)
		{
			foreach (string key in kvp.Keys)
			{
				if (!base.ContainsKey(key))
				{
					base.Add(key, kvp[key]);
				}
			}
		}
		
		public void AddRange(IJsonObject jsonObject)
		{
			AddRange(jsonObject.KeyValuePairs);
		}
		
		public bool ValidateStringKey(string key)
		{
			if (!this.ContainsKey(key))
			{
				return false;
			}
			
			return (this[key] is string);
		}
		
		public void AssertStringKeyExists(string key)
		{
			if (!this.ValidateStringKey(key))
			{
				throw new JsonException(string.Format("Missing string Key: \"{0}\"", key));
			}
		}
		
		public void AssertKeyExists(string key)
		{
			if (!this.ContainsKey(key))
			{
				throw new JsonException("Missing key: " + key);
			}
		}
		
		public bool ValidateKey(string key)
		{
			return this.ContainsKey(key);
		}
		
		public bool ValidateIntKey(string key)
		{
			if (!this.ContainsKey(key))
			{
				return false;
			}
			
			return (this[key] is int);
		}
		
		public void AssertIntKeyExists(string key)
		{
			if (!this.ValidateIntKey(key))
			{
				throw new JsonException(string.Format("Missing integer Key: \"{0}\"", key));
			}
		}
		
		public bool ValidateJsonArrayKey(string key)
		{
			if (!this.ContainsKey(key))
			{
				return false;
			}
			
			return (this[key] is JsonArray);
		}
		
		public void AssertJsonArrayKeyExists(string key)
		{
			if (!this.ValidateJsonArrayKey(key))
			{
				throw new JsonException(string.Format("Missing array Key: \"{0}\"", key));
			}
		}
		
		public bool ValidateFloatKey(string key)
		{
			if (!this.ContainsKey(key))
			{
				return false;
			}
			
			return (this[key] is float) || (this[key] is double) || ValidateIntKey(key);
		}
		
		public void AssertFloatKeyExists(string key)
		{
			if (!this.ValidateFloatKey(key))
			{
				throw new JsonException(string.Format("Missing float Key: \"{0}\"", key));
			}
		}
		
		public bool ValidateBoolKey(string key)
		{
			if (!this.ContainsKey(key))
			{
				return false;
			}
			
			return (this[key] is bool);
		}
		
		public void AssertBooleanKeyExists(string key)
		{
			if (!this.ValidateBoolKey(key))
			{
				throw new JsonException(string.Format("Missing boolean Key: \"{0}\"", key));
			}
		}

		public bool ValidateArrayKey(string key)
		{
			if (!this.ContainsKey(key))
			{
				return false;
			}
			
			return (this[key] is JsonArray);
		}
		
		public void AssertArrayKeyExists(string key)
		{
			if (!this.ValidateArrayKey(key))
			{
				throw new JsonException(string.Format("Missing JsonArray Key: \"{0}\"", key));
			}
		}
		
		public bool ValidateObjectKey(string key)
		{
			if (!this.ContainsKey(key))
			{
				return false;
			}
			
			return (this[key] is JsonKeyValuePairs);
		}
		
		public void AssertObjectKeyExists(string key)
		{
			if (!this.ValidateObjectKey(key))
			{
				throw new JsonException(string.Format("Missing JsonKeyValuePairs Key: \"{0}\"", key));
			}
		}
		
		public override string ToString()
		{
			var text = "";
			var delimitor = "";
			
			foreach (var key in this.Keys)
			{
				text += delimitor + key;
				delimitor = ", ";
			}
			
			return "[" + text + "]";
		}
	}
}
