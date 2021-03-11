using System;
using System.Threading.Tasks;
using Drill4Net.Target.Common;

namespace Drill4Net.Target.Net50
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
                var target = new InjectTarget();
                await target.RunTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
