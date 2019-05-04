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
using System.Linq;

namespace HttpPack
{
	/// <summary>
	/// Description of StringHelper.
	/// </summary>
	public static class StringHelper
	{
		public const string Iso8601 = "yyyy-MM-ddTHH:mm:ss.fffZ";
		
		public static bool IsA<T>(this object obj)
		{
			// from http://stackoverflow.com/questions/811614/c-sharp-is-keyword-and-checking-for-not
			return obj is T;
		}
		
		public static bool IsNumeric(this char c)
		{
			return c.ToString().IsNumeric();
		}
		
		public static bool IsNumeric(this string s)
		{
			float output;
			return float.TryParse(s, out output);
		}
		
		public static string GetString(byte[] bytes)
		{
			if (bytes == null)
			{
				return null;
			}

			// UTF8 byte order mark is: 0xEF,0xBB,0xBF
			if (bytes.Length >= 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
			{
				return System.Text.Encoding.UTF8.GetString(bytes.Skip(3).ToArray());
			}

			return System.Text.Encoding.UTF8.GetString(bytes);
		}
		
		public static string TrimWhiteSpace(this string input)
		{
			if (input == null)
			{
				throw new NullReferenceException("input string is null");
			}
			
			string result = input;

			while (result.StartsWith("\n", StringComparison.InvariantCulture) ||
			       result.StartsWith("\r", StringComparison.InvariantCulture) ||
			       result.StartsWith(" ", StringComparison.InvariantCulture) ||
			       result.StartsWith("\t", StringComparison.InvariantCulture))
			{
				result = StringHelper.TrimStart(result);
			}
			
			while (result.EndsWith("\n", StringComparison.InvariantCulture) ||
			       result.EndsWith("\r", StringComparison.InvariantCulture) ||
			       result.EndsWith(" ", StringComparison.InvariantCulture) ||
			       result.EndsWith("\t", StringComparison.InvariantCulture))
			{
				result = StringHelper.TrimEnd(result);
			}
			
			return result;
		}
		
		public static string TrimStart(string input)
		{
			return TrimStart(input, 1);
		}

		public static string TrimStart(string input, int characters)
		{
			if (String.IsNullOrEmpty(input))
			{
				throw new NullReferenceException("input string is null");
			}

			if (characters > input.Length) characters = input.Length;

			return input.Substring(characters);
		}

		public static string TrimEnd(string input)
		{
			return TrimEnd(input, 1);
		}

		public static string TrimEnd(string input, int characters)
		{
			if (String.IsNullOrEmpty(input))
			{
				throw new NullReferenceException("input string is null");
			}

			if (characters > input.Length) characters = input.Length;

			return input.Substring(0, input.Length - characters);
		}
	}
}
