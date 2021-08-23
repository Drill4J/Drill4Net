using Drill4Net.BanderLog.Sinks.File;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using Serilog;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog.ManualBenchmarks
{
    internal class Tests: IDisposable
    {
        private NLog.Logger _loggerNlog;
        private BanderLog.Logger _loggerBanderLog;
        private log4net.ILog _log4net;
        private string _testString;
        private const string _fileNameSeriLog = "LogFileSerilog.txt";
        private const string _fileNameBanderLog = "LogFileBanderLog.txt";
        private const string _fileNameNLog = "LogFileN.txt";
        private const string _fileNameLog4Net = "LogFileLog4Net.txt";
        internal Tests()
        {
            _testString = new string('a', 100);

            //Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(_fileNameSeriLog)
                .CreateLogger();

            //NLog
            var nlogConfig = new NLog.Config.LoggingConfiguration();
            var nlogFileTarget = new NLog.Targets.FileTarget("logfile") { FileName = _fileNameNLog };
            nlogConfig.AddRuleForAllLevels(nlogFileTarget);
            _loggerNlog = NLog.LogManager.GetCurrentClassLogger();
            NLog.LogManager.Configuration = nlogConfig;

            //log4net
            var layout = new PatternLayout("%date [%thread] %-5level - %message%newline");
            var appender = new RollingFileAppender
            {
                File = _fileNameLog4Net,
                Layout = layout
            };
            layout.ActivateOptions();
            appender.ActivateOptions();
            BasicConfigurator.Configure(appender);
            _log4net = log4net.LogManager.GetLogger(typeof(Tests));

            //BanderLog
            var logBld = new LogBuilder();
            _loggerBanderLog = logBld.AddSink(FileSinkCreator.CreateSink(_fileNameBanderLog)).Build();
        }

        internal void RunMultiTaskTests(int recordCount, int taskCount)
        {
            var startDate = DateTime.Now;
            Targets.UseBanderLogMultiTask(_loggerBanderLog, recordCount, _testString, taskCount);
            var finishDate = DateTime.Now;
            Console.WriteLine($"BanderLogTest. Duration: {(finishDate - startDate).TotalSeconds} sec; " +
                $"Record count: {recordCount}; Task count:{taskCount}.");

            startDate = DateTime.Now;
            Targets.UseSerilogMultiTask(Log.Logger, recordCount, _testString, taskCount);
            finishDate = DateTime.Now;
            Console.WriteLine($"SerilogTest. Duration: {(finishDate - startDate).TotalSeconds} sec; " +
                $"Record count: {recordCount}; Task count:{taskCount}.");

            startDate = DateTime.Now;
            Targets.UseNLogMultiTask(_loggerNlog, recordCount, _testString, taskCount);
            finishDate = DateTime.Now;
            Console.WriteLine($"NLogTest. Duration: {(finishDate - startDate).TotalSeconds} sec; " +
                $"Record count: {recordCount}; Task count:{taskCount}.");

            startDate = DateTime.Now;
            Targets.UseLog4NetMultiTask(_log4net, recordCount, _testString, taskCount);
            finishDate = DateTime.Now;
            Console.WriteLine($"Log4NetTest. Duration: {(finishDate - startDate).TotalSeconds} sec; " +
                $"Record count: {recordCount}; Task count:{taskCount}.");
        }

        internal void RunSimpleTests(int recordCount)
        {
            var startDate = DateTime.Now;
            Targets.UseBanderLog(_loggerBanderLog, recordCount, _testString);
            var finishDate = DateTime.Now;
            Console.WriteLine($"BanderLogTest. Duration: {(finishDate - startDate).TotalSeconds} sec; Record count: {recordCount}.");

            startDate = DateTime.Now;
            Targets.UseSerilog(Log.Logger, recordCount, _testString);
            finishDate = DateTime.Now;
            Console.WriteLine($"SerilogTest. Duration: {(finishDate - startDate).TotalSeconds} sec; Record count: {recordCount}.");

            startDate = DateTime.Now;
            Targets.UseNLog(_loggerNlog, recordCount, _testString);
            finishDate = DateTime.Now;
            Console.WriteLine($"NLogTest. Duration: {(finishDate - startDate).TotalSeconds} sec; Record count: {recordCount}.");

            startDate = DateTime.Now;
            Targets.UseLog4Net(_log4net, recordCount, _testString);
            finishDate = DateTime.Now;
            Console.WriteLine($"Log4NetTest. Duration: {(finishDate - startDate).TotalSeconds} sec; Record count: {recordCount}.");

        }

        public void Dispose()
        {
            Serilog.Log.CloseAndFlush();
            NLog.LogManager.Shutdown();
            _loggerBanderLog.Shutdown();
            log4net.LogManager.Shutdown();
            
            if (File.Exists(_fileNameSeriLog))
                File.Delete(_fileNameSeriLog);

            if (File.Exists(_fileNameBanderLog))
                File.Delete(_fileNameBanderLog);

            if (File.Exists(_fileNameLog4Net))
                File.Delete(_fileNameLog4Net);

            if (File.Exists(_fileNameNLog))
                File.Delete(_fileNameNLog);

            Thread.Sleep(1000);
        }
    }
}
