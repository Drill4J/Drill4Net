using System;
using System.Threading.Tasks;
using Drill4Net.Target.Common;

namespace Drill4Net.Target.Net48.App
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("Press any key when you'll have started the session on Admin side (if needed)");
            Console.ReadKey(true);

            try
            {
                await new InjectTarget().RunTests();
                Console.WriteLine("\nDone.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey(true);
        }
    }
}
