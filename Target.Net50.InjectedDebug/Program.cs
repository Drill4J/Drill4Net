using System;
using System.Threading.Tasks;
using Target.Common;

namespace Target.Net50.InjectedDebug
{
    class Program
    {
        static void Main(string[] args)
        {
            Process().GetAwaiter().GetResult();

            Console.WriteLine("\nDone. SEE LOG!");
            Console.ReadKey(true);
        }

        private static async Task Process()
        {
            try
            {
                var target = new InjectTarget();
                await target.Process();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}
