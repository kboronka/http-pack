using System;

using ApiTools.Json;

namespace ApiTools.Auth
{
	/// <summary>
	/// Description of JwtValidationFailedException.
	/// </summary>
	public class JwtValidationFailedException : JsonException
	{
		public JwtValidationFailedException()
			: base("Validation Failed")
		{
			
		}
	}
}
