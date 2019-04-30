using System;

using HttpPack.Json;

namespace HttpPack.Auth
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
