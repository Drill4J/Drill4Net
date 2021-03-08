using System;

namespace Drill4Net.Injection.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            ProfilerProxy.Process("XXXX");

            //var proxy = new ProfilerProxy();
            //proxy.Process("BBB");

            Console.WriteLine("Done. See log");
            Console.ReadKey(true);
        }
    }
}
