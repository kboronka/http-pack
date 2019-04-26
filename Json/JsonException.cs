using System;

namespace ApiTools.Json
{
	/// <summary>
	/// Description of JsonException.
	/// </summary>
	public class JsonException : Exception
	{
		public JsonException(string message)
			: base(message)
		{
			
		}
	}
}
