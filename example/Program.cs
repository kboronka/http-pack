using System;
using System.IO;

using HttpPack;

namespace example
{
    class Program
    {
        public static void Main()
        {
            TestTLS.TLS11();
            TestBasics.Basics();
            Console.WriteLine();

            var client = new HttpClient<JsonKeyValuePairs>();
            //var r = client.Get("http://localhost:4000/agents", "");
            var kvp = new JsonKeyValuePairs()
            {
                {"name","debug"}
            };

            var r = client.Post("http://localhost:4000/agents/register", kvp, "JWT eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJkYXMtYnVpbGQtYWdlbnQiLCJuYW1lIjoidW5kZWZpbmVkIG5hbWUiLCJwb3J0Ijo0NzAwfQ.jEykpWI7DOlvWEGXaW17awn9LPxDjPSrwCZsvDCVmOw");


            var publicFolder = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\public");
            var port = 4600;
            var server = new HttpServer(port, publicFolder, null);
            Console.WriteLine(string.Format("Listening on port {0}", port));

            var noFolderServer = new HttpServer(port + 1, null, null);
            Console.WriteLine(string.Format("Listening on port {0}", port + 1));


            Console.WriteLine();
            Console.Write("Press any key to stop http server . . . ");
            Console.ReadKey(true);
            Console.WriteLine();
            Console.Write("Shutting down . . . ");
        }
    }
}