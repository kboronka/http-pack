using System;
using System.IO;
using HttpPack.Server;

namespace Example;

internal class Program
{
    public static void Main()
    {
        TestTLS.TLS11();
        TestBasics.Basics();
        Console.WriteLine();

        var publicFolder = Path.Combine(Directory.GetCurrentDirectory(), @"..\..\..\public");
        const int port = 4600;
        var server = new HttpServer(port, publicFolder, null);
        Console.WriteLine("Listening on port {0}", port);

        var noFolderServer = new HttpServer(port + 1, null, null);
        Console.WriteLine("Listening on port {0}", port + 1);


        Console.WriteLine();
        Console.Write("Press any key to stop http server . . . ");
        Console.ReadKey(true);
        Console.WriteLine();
        Console.Write("Shutting down . . . ");
    }
}