/* Copyright (C) 2018 Kevin Boronka
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
 * AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
 * ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
 * LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
 * CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
 * SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
 * CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
 * ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

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