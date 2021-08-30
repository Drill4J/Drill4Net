using System;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog.ManualBenchmarks
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
        public static void UseBanderLogMultiTask(BanderLog.LogManager loggerBanderLog, int recordCount, string logString, int taskCount)
        {
            Task[] tasks = new Task[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                tasks[i] = new Task(() => UseBanderLog(loggerBanderLog, recordCount, logString));
            }

            foreach (var t in tasks)
                t.Start();

            try
            {
                Task.WaitAll(tasks);
            }

            catch (AggregateException ae)
            {
                Utils.WriteAggregateException(ae);
            }
        }
        public static void UseSerilogMultiTask(Serilog.ILogger loggerSerilog, int recordCount, string logString, int taskCount)
        {
            Task[] tasks = new Task[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                tasks[i] = new Task(() => UseSerilog(loggerSerilog, recordCount, logString));
            }

            foreach (var t in tasks)
                t.Start();

            try
            {
                Task.WaitAll(tasks);
            }

            catch (AggregateException ae)
            {
                Utils.WriteAggregateException(ae);
            }
        }
        public static void UseNLogMultiTask(NLog.Logger loggerNLog, int recordCount, string logString, int taskCount)
        {
            Task[] tasks = new Task[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                tasks[i] = new Task(() => UseNLog(loggerNLog, recordCount, logString));
            }

            foreach (var t in tasks)
                t.Start();

            try
            {
                Task.WaitAll(tasks);
            }

            catch (AggregateException ae)
            {
                Utils.WriteAggregateException(ae);
            }
        }
        public static void UseLog4NetMultiTask(log4net.ILog loggerLog4Net, int recordCount, string logString, int taskCount)
        {
            Task[] tasks = new Task[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                tasks[i] = new Task(() => UseLog4Net(loggerLog4Net, recordCount, logString));
            }

            foreach (var t in tasks)
                t.Start();

            try
            {
                Task.WaitAll(tasks);
            }

            catch (AggregateException ae)
            {
                Utils.WriteAggregateException(ae);
            }
        }
    }
}
