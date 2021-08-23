using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Serilog;
using Drill4Net.BanderLog.Sinks.File;
using System.Threading.Tasks;
using log4net;
using log4net.Config;
using System.Reflection;
using log4net.Layout;
using log4net.Appender;

namespace Drill4Net.BanderLog.Benchmarks
{
    /******************************************************************************************
       To run benchmark test build Drill4Net.BanderLog.Benchmarks with Release|Any CPU
     ******************************************************************************************/

    /// <summary>
    /// Benchmarks for comparison of different loggers
    /// </summary>
    [MemoryDiagnoser]
    [HtmlExporter]
    [Config(typeof(Config))]
    public class Tests
    {
        private NLog.Logger _loggerNlog;
        private BanderLog.Logger _loggerBanderLog;
        private log4net.ILog _log4net;
        private string _testString;
        private const string _fileName="LogFile.txt";
        private const string _fileNameSeriLog = "LogFileSerilog.txt";
        private const string _fileNameBanderLog = "LogFileBanderLog.txt";
        private const string _fileNameNLog = "LogFileN.txt";
        private const string _fileNameLog4Net = "LogFileLog4Net.txt";

        /******************************************************************************************/

        private class Config : ManualConfig
        {
            public Config()
            {
                //AddLogger(ConsoleLogger.Default);
                WithOptions(ConfigOptions.DisableOptimizationsValidator);
            }
        }

        [GlobalSetup]
        public void GlobalSetup()
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

        /******************************************************************************************/

        [Benchmark]
        [Arguments(2500)]
        [Arguments(10000)]
        public void BanderLogTest(int recordCount)
        {
            Targets.UseBanderLog(_loggerBanderLog, recordCount, _testString);
        }

        [Benchmark]
        [Arguments(2500)]
        [Arguments(10000)]
        public void SerilogTest(int recordCount)
        {
            Targets.UseSerilog(Log.Logger, recordCount, _testString);
        }

        [Benchmark]
        [Arguments(2500)]
        [Arguments(10000)]
        public void NLogTest(int recordCount)
        {
            Targets.UseNLog(_loggerNlog, recordCount, _testString);
        }

        [Benchmark]
        [Arguments(2500)]
        [Arguments(10000)]
        public void Log4NetTest(int recordCount)
        {
            Targets.UseLog4Net(_log4net, recordCount, _testString);
        }

        //[Benchmark]
        //[Arguments(2500)]
        //public void AppendAllLinesTest(int recordCount)
        //{
        //    var testData = Enumerable.Repeat(_testString, recordCount);
        //    File.AppendAllLines(_fileName, testData);
        //}

        //[Benchmark]
        //[Arguments(2500)]
        //[Arguments(10000)]
        //public void AppendAllTextTest(int recordCount)
        //{
        //    for (var i = 0; i < recordCount; i++)
        //    {
        //        File.AppendAllText(_fileName, _testString);
        //    }
        //}

        [Benchmark]
        [Arguments(2500, 2)]
        [Arguments(2500, 5)]
        [Arguments(2500, 10)]
        public void BanderLogMultiTaskTest(int recordCount, int taskCount)
        {
            Task[] tasks = new Task[taskCount];
            for (var i = 0; i < taskCount; i++)
            {
                tasks[i] = new Task(() => Targets.UseBanderLog(_loggerBanderLog, recordCount, _testString));
            }

            foreach (var t in tasks)
                t.Start();

            try
            {
                Task.WaitAll(tasks);
            }

            catch (AggregateException ae)
            {
                Console.WriteLine("An exception occurred:");
                foreach (var ex in ae.Flatten().InnerExceptions)
                    Console.WriteLine("   {0}", ex.Message);
            }
        }

        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Serilog.Log.CloseAndFlush();
            NLog.LogManager.Shutdown();
            _loggerBanderLog.Shutdown();
            log4net.LogManager.Shutdown();
        }
    }
}
