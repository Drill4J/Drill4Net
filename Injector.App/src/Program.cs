using System;
using Injector.Engine;

namespace Injector.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
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
