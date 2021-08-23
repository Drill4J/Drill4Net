using System;

namespace Drill4Net.BanderLog.ManualBenchmarks
{
    /// <summary>
    /// Utils for Manual Benchmarks
    /// </summary>
    static class Utils
    {
        /// <summary>
        /// Write aggrecate exception to console
        /// </summary>
        ///<param name="ae">Aggregate Exception</param>
        public static void WriteAggregateException(AggregateException ae)
        {
            Console.WriteLine("An exception occurred:");
            foreach (var ex in ae.Flatten().InnerExceptions)
                Console.WriteLine("   {0}", ex.Message);
        }
    }
}
