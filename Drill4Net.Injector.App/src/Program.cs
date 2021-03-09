using System;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Injector.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Drill4Net is started");

                var injector = new InjectorEngine();
                injector.Process(args);

                Console.WriteLine("Injection is done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadKey(true);
        }
    }
}
