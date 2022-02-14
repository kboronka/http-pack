using HttpPack.Json;

namespace HttpPack.Auth;

/// <summary>
///     Description of JwtValidationFailedException.
/// </summary>
public class JwtValidationFailedException : JsonException
{
    public JwtValidationFailedException()
        : base("Validation Failed")
    {
    }
}