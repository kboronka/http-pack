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

namespace HttpPack.Json
{
	/// <summary>
	/// Description of JsonCollection.
	/// </summary>
	public class JsonCollection : JsonKeyValuePairs
	{
		public JsonCollection() : base()
		{
			
		}
		
		public JsonCollection(string json)
			: base()
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
							this.Add(key, JsonHelper.ValueToObject(value));
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
}
