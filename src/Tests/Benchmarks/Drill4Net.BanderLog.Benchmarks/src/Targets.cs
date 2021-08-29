using System;

namespace Drill4Net.BanderLog.Benchmarks
{
    static class Targets
    {
        public static void UseBanderLog(BanderLog.LogManager loggerBanderLog, int recordCount, string logString)
        {
            for (var i = 0; i < recordCount; i++)
            {
                loggerBanderLog.Log(Microsoft.Extensions.Logging.LogLevel.Information, logString);
            }
        }
        public static void UseSerilog(Serilog.ILogger loggerSerilog, int recordCount, string logString)
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
        public static void UseLog4Net(log4net.ILog loggerLog4Net, int recordCount, string logString)
        {
            for (var i = 0; i < recordCount; i++)
            {
                loggerLog4Net.Info(logString);
            }
        }
    }
}
