using System;

namespace HttpPack
{
	/// <summary>
	/// Description of JwtValidationFailedException.
	/// </summary>
	public class JwtDecodingException : JsonException
	{
		public JwtDecodingException(string message)
			: base(message)
		{
			
		}
	}
}
