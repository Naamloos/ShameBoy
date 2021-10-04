using System;
using ShameBoy;

namespace ShameBoy.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var boy = new GameBoy(new byte[256]);
            Console.WriteLine("Press the any key to exit.");
            Console.ReadKey();
        }
    }
}
