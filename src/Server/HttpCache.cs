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
using System.Collections.Generic;
using System.IO;

namespace HttpPack
{
    public class HttpCache
    {
        private readonly Dictionary<string, HttpCachedFile> cache;
        private readonly HttpServer server;

        public HttpCache(HttpServer server)
        {
            cache = new Dictionary<string, HttpCachedFile>();
            this.server = server;

            // read all files on root folder
            if (server.Root != null)
            {
                foreach (var file in GetAllFiles(server.Root))
                {
                    var request = file.Substring(server.Root.Length + 1).ToLower();
                    cache.Add(request, new HttpCachedFile(file));
                }
            }
        }

        public List<string> Files
        {
            get
            {
                var result = new List<string>();

                foreach (var file in cache)
                {
                    result.Add(file.Key);
                }

                return result;
            }
        }

        public bool Contains(HttpRequest request)
        {
            if (server.Root == null)
            {
                return false;
            }

            var requestPath = request.Path;
            var filePath = Path.Combine(server.Root, requestPath.Replace(@"/", @"\"));

            return Contains(filePath);
        }

        public bool Contains(string filePath)
        {
            return cache.ContainsKey(filePath);
        }

        public HttpCachedFile Get(HttpRequest request)
        {
            var requestPath = request.Path;

            var filePath = Path.Combine(server.Root, requestPath.Replace(@"/", @"\"));
            return Get(filePath);
        }

        public bool Find(string filePath)
        {
            return cache.ContainsKey(filePath) || File.Exists(filePath);
        }

        public HttpCachedFile Get(string filePath)
        {
            if (cache.ContainsKey(filePath))
            {
                return cache[filePath];
            }

            // TODO: this doesn't work
            if (File.Exists(filePath))
            {
                var request = filePath.Substring(server.Root.Length + 1).ToLower();
                var newFile = new HttpCachedFile(filePath);
                cache.Add(request, newFile);

                return newFile;
            }

            throw new FileNotFoundException(filePath);
        }

        public static List<string> GetAllFiles(string root)
        {
            if (string.IsNullOrEmpty(root))
            {
                throw new NullReferenceException("root search path was not specified");
            }

            var pattern = "*.*";

            // handle filepaths in root
            if (!Directory.Exists(root) && root.Contains("*"))
            {
                pattern = root.Substring(root.LastIndexOf('\\') + 1);
                root = root.Substring(0, root.LastIndexOf('\\'));
            }

            return GetAllFiles(root, pattern);
        }

        public static List<string> GetAllFiles(string root, string pattern)
        {
            var files = new List<string>();
            var dirs = GetAllDirectories(root);
            foreach (var dir in dirs)
            {
                try
                {
                    files.AddRange(Directory.GetFiles(dir, pattern));
                }
                catch
                {
                }
            }

            return files;
        }

        private static List<string> GetAllDirectories(string root)
        {
            var directories = new List<string>();

            if (Directory.Exists(root))
            {
                directories.Add(root);

                foreach (var dir in Directory.GetDirectories(root))
                {
                    directories.AddRange(GetAllDirectories(dir));
                }
            }

            return directories;
        }
    }
}