using System;

using ApiTools.Json;

namespace ApiTools.Auth
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
