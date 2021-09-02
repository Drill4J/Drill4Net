using System;
using Drill4Net.Injector.App.Helpers.Interfaces;

namespace Drill4Net.Injector.App.Helpers
{
    /// <summary>
    /// Helper for writing benchmark information to separate log
    /// </summary>
    internal static class BenchmarkLog
    {
        /// <summary>
        /// Write benchmark information to log
        /// </summary>
        /// <param name="logger">Logger for writing data</param>
        /// <param name="gitBranch">Git branch name for benchmarked method</param>
        ///<param name="gitCommit">Git commit for benchmarked method</param>
        /// <param name="benchmarkData">Data collected by benchmark</param>
        /// <returns></returns>
        internal static void WriteBenchmarkToLog(IBenchmarkLogger logger,string gitBranch, string gitCommit, string benchmarkData)
        {
            var msg = $"{DateTime.Now}|{gitBranch}|{gitCommit}|{benchmarkData}";
            logger.WriteBenchmarkToLog(msg);
        }
    }
}
