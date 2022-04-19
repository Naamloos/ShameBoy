using System;
using ShameBoy;

namespace ShameBoy.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            var boy = new GameBoy();
            boy.Start();
            Console.WriteLine("Press the any key to exit.");
            Console.ReadKey();
        }
    }
}
