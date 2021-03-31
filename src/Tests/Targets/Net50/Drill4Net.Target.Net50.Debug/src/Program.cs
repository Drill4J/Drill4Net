using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Drill4Net.Target.Interfaces;

namespace Drill4Net.Target.Net50.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            StartAsync().GetAwaiter().GetResult();

            Console.WriteLine("\nDone.");
            Console.ReadKey(true);
        }

        private static async Task StartAsync()
        {
            try
            {
                await Start(new Common.InjectTarget());
                await Start(new Net50.InjectTarget());
                await Start(new Core31.InjectTarget());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static async Task Start(IInjectTarget target)
        {
            PrintInfo(target);
            await target.RunTests();
        }

        private static void PrintInfo([NotNull]object obj)
        {
            Console.WriteLine($"\n*** {obj.GetType().FullName} ***\n");
        }
    }
}
