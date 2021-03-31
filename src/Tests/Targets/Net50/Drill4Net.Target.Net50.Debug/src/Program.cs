using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

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
                var targetCommon = new Common.InjectTarget();
                PrintInfo(targetCommon);
                await targetCommon.RunTests();

                var target50 = new InjectTarget();
                PrintInfo(target50);
                await target50.RunTests();

                var target31 = new Core31.InjectTarget();
                PrintInfo(target31);
                await target31.RunTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void PrintInfo([NotNull]object obj)
        {
            Console.WriteLine($"\n*** {obj.GetType().FullName} ***\n");
        }
    }
}
