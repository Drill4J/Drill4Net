using System;
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
                await targetCommon.RunTests();

                var target50 = new InjectTarget();
                await target50.RunTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
