using System;
using System.Linq;
using System.Text;

namespace HttpPack.Json;

/// <summary>
///     Description of StringHelper.
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
            return Encoding.UTF8.GetString(bytes.Skip(3).ToArray());
        }

        return Encoding.UTF8.GetString(bytes);
    }

    public static string TrimWhiteSpace(this string input)
    {
        if (input == null)
        {
            throw new NullReferenceException("input string is null");
        }

        var result = input;

        while (result.StartsWith("\n", StringComparison.InvariantCulture) ||
               result.StartsWith("\r", StringComparison.InvariantCulture) ||
               result.StartsWith(" ", StringComparison.InvariantCulture) ||
               result.StartsWith("\t", StringComparison.InvariantCulture))
        {
            result = TrimStart(result);
        }

        while (result.EndsWith("\n", StringComparison.InvariantCulture) ||
               result.EndsWith("\r", StringComparison.InvariantCulture) ||
               result.EndsWith(" ", StringComparison.InvariantCulture) ||
               result.EndsWith("\t", StringComparison.InvariantCulture))
        {
            result = TrimEnd(result);
        }

        return result;
    }

    public static string TrimStart(string input)
    {
        return TrimStart(input, 1);
    }

    public static string TrimStart(string input, int characters)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new NullReferenceException("input string is null");
        }

        if (characters > input.Length)
        {
            characters = input.Length;
        }

        return input.Substring(characters);
    }

    public static string TrimEnd(string input)
    {
        return TrimEnd(input, 1);
    }

    public static string TrimEnd(string input, int characters)
    {
        if (string.IsNullOrEmpty(input))
        {
            throw new NullReferenceException("input string is null");
        }

        if (characters > input.Length)
        {
            characters = input.Length;
        }

        return input.Substring(0, input.Length - characters);
    }
}