using System;
using System.IO;
using System.Text.RegularExpressions;

namespace HttpPack.Utils
{
    public static class ExceptionHelper
    {
        public static string GetStackTrace(Exception ex)
        {
            try
            {
                if (ex.StackTrace == null)
                {
                    return "[StackTrace not available]";
                }

                var result = "";
                var stackTrace = ex.StackTrace;
                var regex = @"(\s*)at\s((.?)*)\sin\s((.?)*):line\s(\d*)";

                var lines = stackTrace.Split(new[] {"\r\n", "\n"}, StringSplitOptions.None);

                foreach (var line in lines)
                {
                    var regexResults = Regex.Matches(line, regex);
                    if (regexResults.Count == 1 && regexResults[0].Groups.Count == 7)
                    {
                        if (!string.IsNullOrEmpty(result))
                        {
                            result += Environment.NewLine;
                        }

                        var method = regexResults[0].Groups[2].Value;
                        var filepath = regexResults[0].Groups[4].Value;
                        var lineNumber = regexResults[0].Groups[6].Value;

                        var filename = Path.GetFileName(filepath);

                        if (method.Contains("."))
                        {
                            method = method.Substring(method.LastIndexOf('.') + 1);
                        }

                        if (method.Contains("("))
                        {
                            method = method.Substring(0, method.LastIndexOf('('));
                        }

                        result += "\t" + method + "() in " + filename + ":line " + lineNumber;
                    }
                }

                return result;
            }
            catch
            {
                return "[StackTrace not available]";
            }
        }

        public static Exception GetInner(Exception ex)
        {
            while (ex.InnerException != null)
            {
                ex = ex.InnerException;
            }

            return ex;
        }
    }
}