using System;
using HttpPack;

namespace example
{
    public static class TestBasics
    {
        public static bool Basics()
        {
            var postOne = GetExample(1);
            Console.WriteLine("GET Example response: " + postOne.Stringify());
            Console.WriteLine();

            var newPost = new Post("foo", "bar", 1);
            var postResponse = PostExample(newPost, "");
            Console.WriteLine("POST Example response: " + postResponse);
            Console.WriteLine();

            return true;
        }

        /// <summary>
        ///     HTTP GET example
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private static JsonKeyValuePairs GetExample(int id)
        {
            var uri = string.Format("https://jsonplaceholder.typicode.com/posts/{0}", id);
            var client = new HttpClient<JsonKeyValuePairs>();
            var res = client.Get(uri, "");

            if (res.Code == 200)
            {
                return res.Body;
            }

            return null;
        }

        /// <summary>
        ///     HTTP POST example
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        private static int PostExample(Post post, string auth)
        {
            var req = new JsonKeyValuePairs
            {
                {"body", post}
            };

            var uri = "https://jsonplaceholder.typicode.com/posts";
            var client = new HttpClient<JsonKeyValuePairs>();
            var res = client.Post(uri, req, auth);

            if (res.Code == 201) // 201 = Created
            {
                return (int) res.Body["id"];
            }

            return -1;
        }
    }
}