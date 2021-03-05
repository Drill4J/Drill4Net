using System;

namespace Injector.Injection.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Drill4J.Injection.ProfilerProxy.Process("AAAAA");

            Console.WriteLine("Done. See log");
            Console.ReadKey(true);
        }
    }
}
