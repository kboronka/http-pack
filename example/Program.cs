using System;

namespace example
{
	class Program
	{
		public static void Main(string[] args)
		{
            TestTLS.TLS11();
            TestBasics.Basics();

            Console.Write("Press any key to continue . . . ");
			Console.ReadKey(true);
		}
    }
}