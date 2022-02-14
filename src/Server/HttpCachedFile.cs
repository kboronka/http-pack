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
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace HttpPack
{
    public class HttpCachedFile
    {
        private readonly FileSystemWatcher watcher;

        protected bool embedded;
        protected string path;
        public DateTime LastModified { get; protected set; }
        public string ContentType { get; }
        public string ETag { get; private set; }
        public byte[] Data { get; private set; }

        public bool ParsingRequired { get; protected set; }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            Data = ReadAllBytes(path);
            ETag = GetETag(Data);
            LastModified = File.GetLastWriteTimeUtc(path);
        }

        private void OnDelete(object sender, FileSystemEventArgs e)
        {
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
        }

        private static string GetETag(byte[] data)
        {
            var hash = new MD5CryptoServiceProvider().ComputeHash(data);
            var hex = "";

            foreach (var b in hash)
            {
                hex += b.ToString("X2");
            }

            return @""" + hex + @""";
        }

        private static byte[] ReadAllBytes(string path)
        {
            byte[] buffer;

            using (var fs = WaitForFile(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                buffer = new byte[fs.Length];
                fs.Read(buffer, 0, (int) fs.Length);
            }

            return buffer;
        }

        private static FileStream WaitForFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            // check if file is locked by another application
            for (var attempts = 0; attempts < 10; attempts++)
            {
                try
                {
                    var fs = new FileStream(path, mode, access, share);

                    fs.ReadByte();
                    fs.Seek(0, SeekOrigin.Begin);

                    return fs;
                }
                catch (IOException)
                {
                    Thread.Sleep(50);
                }
            }

            return null;
        }

        #region constructors

        protected HttpCachedFile(string path, byte[] data)
        {
            this.path = path;
            embedded = true;

            var extension = Path.GetExtension(path).ToLower();
            ContentType = HttpMimeTypes.GetMimeType(extension);
            Data = data;
            ETag = GetETag(Data);
            ParsingRequired = false;

            if (ContentType.Contains("text") || ContentType.Contains("xml"))
            {
                var text = Encoding.ASCII.GetString(Data);
                var matches = Regex.Matches(text, HttpContent.INCLUDE_RENDER_SYNTAX);
                if (matches.Count > 0)
                {
                    ParsingRequired = true;
                }

                // include linked externals
                matches = Regex.Matches(text, HttpContent.CONTENT_RENDER_SYNTAX);
                if (matches.Count > 0)
                {
                    ParsingRequired = true;
                }
            }
        }

        public HttpCachedFile(string path) : this(path, File.ReadAllBytes(path))
        {
            LastModified = File.GetLastWriteTimeUtc(path);

            watcher = new FileSystemWatcher
            {
                Path = Path.GetDirectoryName(path),
                Filter = Path.GetFileName(path),
                NotifyFilter = NotifyFilters.LastWrite
            };
            watcher.Changed += OnChanged;
            watcher.Deleted += OnDelete;
            watcher.Renamed += OnRenamed;
            watcher.EnableRaisingEvents = true;
        }

        #endregion
    }
}