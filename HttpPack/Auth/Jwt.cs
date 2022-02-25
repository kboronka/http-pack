using System;
using System.Security.Cryptography;
using System.Text;
using HttpPack.Json;

namespace HttpPack.Auth;

/// <summary>
///     JSON Web Token
/// </summary>
public class Jwt
{
    public Jwt(JsonKeyValuePairs payload, string secret)
    {
        var header = CreateJwtHeader();

        var jwt = Base64UrlEncode(header.Stringify()) + "." + Base64UrlEncode(payload.Stringify());
        jwt += "." + Sign(jwt, secret);

        Token = jwt;
    }

    public Jwt(JsonArray payload, string secret)
    {
        var header = CreateJwtHeader();

        var jwt = Base64UrlEncode(header.Stringify()) + "." + Base64UrlEncode(payload.ToJson());
        jwt += "." + Sign(jwt, secret);

        Token = jwt;
    }

    public Jwt(string token, string secret)
    {
        Token = token;
        var segments = token.Split('.');

        if (segments.Length != 3)
        {
            throw new Exception("Token structure is incorrect!");
        }

        var header = new JsonKeyValuePairs(Encoding.UTF8.GetString(Base64UrlDecode(segments[0])));
        var payload = new JsonKeyValuePairs(Encoding.UTF8.GetString(Base64UrlDecode(segments[1])));

        var rawSignature = segments[0] + '.' + segments[1];

        if (!Verify(rawSignature, secret, segments[2]))
        {
            throw new JwtValidationFailedException();
        }

        Payload = payload;
    }

    public JsonKeyValuePairs Payload { get; }
    public string Secret { get; private set; }
    public string Token { get; }

    private JsonKeyValuePairs CreateJwtHeader()
    {
        const string algorithm = "HS256";

        var header = new JsonKeyValuePairs
        {
            {"alg", algorithm},
            {"typ", "JWT"}
        };

        return header;
    }

    private static bool Verify(string rawSignature, string secret, string signature)
    {
        var newSignature = Sign(rawSignature, secret);
        return signature == newSignature;
    }

    private static string Sign(string str, string key)
    {
        var encoding = new ASCIIEncoding();
        byte[] signature;

        using (var crypto = new HMACSHA256(encoding.GetBytes(key)))
        {
            signature = crypto.ComputeHash(encoding.GetBytes(str));
        }

        return Base64UrlEncode(signature);
    }

    private static string Base64UrlEncode(string obj)
    {
        return Base64UrlEncode(Encoding.UTF8.GetBytes(obj));
    }

    private static string Base64UrlEncode(byte[] arg)
    {
        var base64 = Convert.ToBase64String(arg);
        base64 = base64.Split('=')[0];
        base64 = base64.Replace('+', '-');
        base64 = base64.Replace('/', '_');
        return base64;
    }

    private static byte[] Base64UrlDecode(string base64)
    {
        base64 = base64.Replace('-', '+');
        base64 = base64.Replace('_', '/');

        // Pad with trailing '='s
        switch (base64.Length % 4)
        {
            case 0:
                break;
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
            default:
                throw new JwtDecodingException("Illegal base64url string!");
        }

        return Convert.FromBase64String(base64);
    }
}