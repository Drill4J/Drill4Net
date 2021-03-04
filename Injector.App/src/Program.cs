using System;
using Injector.Core;

namespace TestA.Interceptor
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
