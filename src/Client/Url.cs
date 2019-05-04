using System;

namespace HttpPack.Client
{
    public static class Url
    {
        public static string Combine(params string[] parts)
        {
            if (parts == null)
            {
                throw new ArgumentNullException(nameof(parts));
            }

            var result = "";
            foreach (var part in parts)
            {
                if (string.IsNullOrEmpty(part))
                {
                    continue;
                }

                result = CombineUrlParts(result, part);
            }

            return EscapeUriString(result);
        }

        private static string CombineUrlParts(string a, string b)
        {
            char seperator = '/';

            if (string.IsNullOrEmpty(a)) return b;
            if (string.IsNullOrEmpty(b)) return a;
            return a.TrimEnd(seperator) + seperator + b.TrimStart(seperator);
        }

        public static string EscapeUriString(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return s;
            }

            s = s.Replace(" ", "+");
            return Uri.EscapeUriString(s);
        }
    }
}