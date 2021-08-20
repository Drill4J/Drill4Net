using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog.Tests
{
    /// <summary>
    /// Utils for Banderlog Tests
    /// </summary>
    static class BanderlogTestsUtils
    {
        /// <summary>
        /// Write aggrecate exception to console
        /// </summary>
        ///<param name="ae">(Aggregate Exceptionr</param>
        public static void WriteAggregateException(AggregateException ae)
        {
            Console.WriteLine("An exception occurred:");
            foreach (var ex in ae.Flatten().InnerExceptions)
                Console.WriteLine("   {0}", ex.Message);
        }
    }
}
