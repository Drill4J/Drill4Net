using System;

namespace Drill4Net.Common
{
    /// <summary>
    /// Helper for writing benchmark information to separate log
    /// </summary>
    public static class BenchmarkLog
    {
        /// <summary>
        /// Write benchmark information to log
        /// </summary>
        /// <param name="logger">Logger for writing data</param>
        /// <param name="gitBranch">Git branch name for benchmarked method</param>
        ///<param name="gitCommit">Git commit for benchmarked method</param>
        /// <param name="benchmarkData">Data collected by benchmark</param>
        /// <returns></returns>
        public static void WriteBenchmarkToLog(IBenchmarkLogger logger,string gitBranch, string gitCommit, string benchmarkData)
        {
            var msg = $"{DateTime.Now}|{gitBranch}|{gitCommit}|{benchmarkData}";
            logger.WriteBenchmarkToLog(msg);
        }
    }
}
