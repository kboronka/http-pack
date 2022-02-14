using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace HttpPack
{
    public class HttpContent
    {
        protected Dictionary<string, HttpContent> baseContent;

        protected byte[] content;

        protected HttpContent() : this(Encoding.ASCII.GetBytes(""), "text/plain")
        {
        }

        public HttpContent(string content) : this(Encoding.ASCII.GetBytes(content), "text/plain")
        {
        }

        public HttpContent(Dictionary<string, object> kvp) : this(Encoding.ASCII.GetBytes(kvp.ToJson()),
            "application/json")
        {
        }

        public HttpContent(List<Dictionary<string, object>> list) : this(Encoding.ASCII.GetBytes(list.ToJson()),
            "application/json")
        {
        }

        public HttpContent(byte[] content, string contentType)
        {
            baseContent = new Dictionary<string, HttpContent>();
            ContentType = contentType;
            this.content = content;
            ParsingRequired = false;
            LastModified = DateTime.UtcNow;
            ETag = "";
        }

        public HttpContent(HttpCachedFile file) : this(file, new Dictionary<string, HttpContent>())
        {
        }

        public HttpContent(HttpCachedFile file, Dictionary<string, HttpContent> baseContent)
        {
            this.baseContent = baseContent;
            content = file.Data;
            ParsingRequired = file.ParsingRequired;
            LastModified = file.LastModified;
            ContentType = file.ContentType;
            ETag = file.ETag;
        }

        public DateTime LastModified { get; protected set; }
        public string ContentType { get; protected set; }
        public string ETag { get; }
        public bool ParsingRequired { get; protected set; }

        #region static

        public static HttpContent Read(HttpRequest request, string requestView)
        {
            return Read(request, requestView, new Dictionary<string, HttpContent>());
        }

        public static HttpContent Read(HttpRequest request, string requestView,
            Dictionary<string, HttpContent> baseContent)
        {
            return Read(request.Server, requestView, baseContent);
        }

        public static HttpContent Read(HttpServer server, string request)
        {
            return Read(server, request, new Dictionary<string, HttpContent>());
        }

        public static HttpContent Read(HttpServer server, string request, Dictionary<string, HttpContent> baseContent)
        {
            request = request.TrimWhiteSpace().Replace(@"/", @"\").ToLower();
            var filePath = server.Root + @"\" + request;

            if (server.Cache.Contains(request))
            {
                return new HttpContent(server.Cache.Get(request), baseContent);
            }

            if (File.Exists(filePath))
            {
                return new HttpContent(server.Cache.Get(filePath), baseContent);
            }

            if (filePath.EndsWith("favicon.ico"))
            {
                return new HttpContent(server.Cache.Get("HttpPack.Server.libs.art.favicon.ico"), baseContent);
            }

            throw new FileNotFoundException("did not find " + filePath);
        }

        private static HttpContent Read(HttpCache cache, string request, Dictionary<string, HttpContent> baseContent)
        {
            request = request.TrimWhiteSpace().Replace(@"/", @"\").ToLower();

            if (cache.Contains(request))
            {
                return new HttpContent(cache.Get(request));
            }

            throw new FileNotFoundException("did not find " + request);
        }

        private static byte[] GetFile(string filepath)
        {
            return File.ReadAllBytes(filepath);
        }

        #endregion

        #region render

        public byte[] Render(HttpCache cache)
        {
            return ParsingRequired ? Render(cache, baseContent) : content;
        }

        private string RenderText(HttpCache cache, Dictionary<string, HttpContent> baseContent)
        {
            return StringHelper.GetString(Render(cache, baseContent));
        }

        public const string INCLUDE_RENDER_SYNTAX = @"\<%@ Include:\s*([^@]+)\s*\%\>";
        public const string CONTENT_RENDER_SYNTAX = @"\<%@ Content:\s*([^@]+)\s*\%\>";

        private byte[] Render(HttpCache cache, Dictionary<string, HttpContent> baseContent)
        {
            if (ContentType.Contains("text") || ContentType.Contains("xml"))
            {
                var text = Encoding.ASCII.GetString(content);

                // include linked externals
                var matches = Regex.Matches(text, INCLUDE_RENDER_SYNTAX);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var key = match.Groups[1].Value.TrimWhiteSpace();
                        var replacmentContent = Read(cache, key, baseContent).RenderText(cache, baseContent);
                        text = Regex.Replace(text, match.Groups[0].Value, replacmentContent);
                    }
                }

                // include linked externals
                matches = Regex.Matches(text, CONTENT_RENDER_SYNTAX);
                if (matches.Count > 0)
                {
                    foreach (Match match in matches)
                    {
                        var key = match.Groups[1].Value.TrimWhiteSpace();
                        if (baseContent.ContainsKey(key))
                        {
                            var replacmentContent = baseContent[key];
                            text = Regex.Replace(text, match.Groups[0].Value,
                                replacmentContent.RenderText(cache, baseContent));
                        }
                    }
                }

                return Encoding.ASCII.GetBytes(text);
            }

            return content;
        }

        #endregion
    }
}