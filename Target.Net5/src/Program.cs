using System;
using Target.Common;

namespace Target.Net5
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var target = new InjectTarget();
                target.Process();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.ReadKey(true);
        }

    }
}
