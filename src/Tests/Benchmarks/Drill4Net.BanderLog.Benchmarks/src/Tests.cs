using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Serilog;
using Drill4Net.BanderLog.Sinks.File;

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
        [Params(2500, 10000)]
        public int RecordCount { get; set; }

        private NLog.Logger _loggerNlog;
        private BanderLog.Logger _loggerBanderLog;

        private IEnumerable<string> _testData;
        private string _testString;
        private const string _fileName="LogFile.txt";
        private const string _fileNameSeriLog = "LogFileSerilog.txt";
        private const string _fileNameBanderLog = "LogFileBanderLog.txt";

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
            _testString = new string('a', 50);
            _testData = Enumerable.Repeat(_testString, RecordCount);

            //Serilog
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(_fileNameSeriLog)
                .CreateLogger();

            //NLog
            _loggerNlog = NLog.LogManager.GetCurrentClassLogger();

            //BanderLog
            var logBld = new LogBuilder();
            _loggerBanderLog = logBld.AddSink(FileSinkCreator.CreateSink(_fileNameBanderLog)).Build();
        }

        /******************************************************************************************/

        [Benchmark]
        public void UseBanderLog()
        {
            for (var i = 0; i < RecordCount; i++)
            {
                _loggerBanderLog.Log(Microsoft.Extensions.Logging.LogLevel.Information, _testString);
            }
        }

        [Benchmark]
        public void UseSerilog()
        {
            for (var i = 0; i < RecordCount; i++)
            {
                Log.Logger.Information(_testString);
            }
        }

        [Benchmark]
        public void UseNLog()
        {
            for (var i = 0; i < RecordCount; i++)
            {
                _loggerNlog.Info(_testString);
            }
        }

        //[Benchmark]
        //public void UseAppendAllLines()
        //{
        //    File.AppendAllLines(_fileName, _testData);
        //}

        //[Benchmark]
        //public void UseAppendAllText()
        //{
        //    for (var i = 0; i < RecordCount; i++)
        //    {
        //        File.AppendAllText(_fileName, _testString);
        //    }
        //}

        //[Benchmark]
        //public void WriteLogWithBanderLog5yTasks()
        //{
        //    //Task[] tasks = new Task[2];
        //    //for (var i = 0; i < tasks.Length; i++)
        //    //{
        //    //    tasks[i] = Task.Run(() => UseBanderLog());
        //    //}
        //    //Task.WaitAll(tasks);
        //}

        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Serilog.Log.CloseAndFlush();
            NLog.LogManager.Shutdown();
            _loggerBanderLog.Shutdown();
        }
    }
}
