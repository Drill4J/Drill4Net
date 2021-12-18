using System;
using System.Threading.Tasks;
using Drill4Net.Target.Common;

namespace Drill4Net.Target.Core31.App
{
    static class Program
    {
        public static async Task Main()
        {
            Console.WriteLine("Press any key when you'll have attached the debugger (if needed)");
            Console.ReadKey(true);

            try
            {
                await new ModelTarget().RunTests();
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
