using System;
using System.IO;

using HttpPack.Server;

namespace example
{
    class Program
    {
        public static void Main()
        {
            TestTLS.TLS11();
            TestBasics.Basics();

            var publicFolder = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\public");
            var server = new HttpServer(4600, publicFolder, null);

            var noFolderServer = new HttpServer(4601, null, null);

            Console.Write("Press any key to stop http server . . . ");
            Console.ReadKey(true);
        }
    }
}