using System;

namespace HttpPack
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
