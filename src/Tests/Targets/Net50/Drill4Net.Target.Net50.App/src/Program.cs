using System;
using System.Threading.Tasks;
using Drill4Net.Target.Common;

namespace Drill4Net.Target.Net50.App
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press any key when you'll have started the session on Admin side (if needed)");
            Console.ReadKey(true);

            StartAsync().GetAwaiter().GetResult();

            Console.WriteLine("\nDone.");
            Console.ReadKey(true);
        }

        private static async Task StartAsync()
        {
            try
            {
                await new InjectTarget().RunTests();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
