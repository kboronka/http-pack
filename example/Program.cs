using System;
using System.IO;

using HttpPack.Server;

namespace example
{
	class Program
	{
		public static void Main(string[] args)
		{
            TestTLS.TLS11();
            TestBasics.Basics();

            var publicFolder = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\public");
            var server = new HttpServer(4600, publicFolder, null);
            
            Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
    }
}