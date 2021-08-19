using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog.Tests
{
    static class BanderlogTestsUtils
    {
        public static void WriteAggregateException(AggregateException ae)
        {
            Console.WriteLine("An exception occurred:");
            foreach (var ex in ae.Flatten().InnerExceptions)
                Console.WriteLine("   {0}", ex.Message);
        }
    }
}
