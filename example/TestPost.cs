using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

using HttpPack.Json;
using HttpPack.Net;

namespace example
{
    public static class TestBasics
    {
        public static bool Basics()
        {
            var postOne = HttpGet(1);
            Console.WriteLine(postOne.Stringify());

            var newPost = new Post("foo", "bar", 1);
            HttpPost(newPost, "");

            return true;
        }
        /// <summary>
        /// HTTP GET example
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static JsonKeyValuePairs HttpGet(int id)
        {
            var uri = string.Format("https://jsonplaceholder.typicode.com/posts/{0}", id);

            Console.WriteLine("");
            Console.WriteLine("HttpGet -- " + uri);
            var client = new RestClient();
            var res = client.Fetch(uri);

            if (res.Code == 200)
            {
                Console.WriteLine("  " + res.Body);
                return new JsonKeyValuePairs(res.Body);
            }

            return null;
        }

        /// <summary>
        /// HTTP POST example
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        private static int HttpPost(Post post, string auth)
        {
            var req = new JsonKeyValuePairs
            {
                { "body", post }
            };

            var uri = "https://jsonplaceholder.typicode.com/posts";
            Console.WriteLine("");
            Console.WriteLine("HttpPost -- " + uri);

            var client = new RestClient();
            var res = client.Fetch(uri, WebRequestMethods.Http.Post, req, auth);

            if (res.Code == 201)    // 201 = Created
            {
                Console.WriteLine("  " + res.Body);
                var kvp = new JsonKeyValuePairs(res.Body);
                return (int)kvp["id"];
            }
            else
            {
                return -1;
            }
        }

    }
}
