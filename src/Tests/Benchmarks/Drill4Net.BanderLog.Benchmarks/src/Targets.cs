using System;

namespace Drill4Net.BanderLog.Benchmarks
{
    static class Targets
    {
        public static void UseBanderLog(BanderLog.Logger loggerBanderLog, int recordCount, string logString)
        {
            for (var i = 0; i < recordCount; i++)
            {
                loggerBanderLog.Log(Microsoft.Extensions.Logging.LogLevel.Information, logString);
            }
        }
        public static void UseSerilog(Serilog.ILogger loggerSerilog , int recordCount, string logString)
        {
            for (var i = 0; i < recordCount; i++)
            {
                loggerSerilog.Information(logString);
            }
        }
        public static void UseNLog(NLog.Logger loggerNLog, int recordCount, string logString)
        {
            for (var i = 0; i < recordCount; i++)
            {
                loggerNLog.Info(logString);
            }
        }
    }
}
