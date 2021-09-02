using System;
using Drill4Net.Injector.App.Helpers.Interfaces;

namespace Drill4Net.Injector.App.Helpers
{
   internal static class BenchmarkLog
    {
        internal static void WriteBenchmarkToLog(IBenchmarkLogger logger,string gitBranch, string gitCommit, string benchmarkData)
        {
            var msg = $"{DateTime.Now}|{gitBranch}|{gitCommit}|{benchmarkData}";
            logger.WriteBenchmarkToLog(msg);
        }
    }
}
