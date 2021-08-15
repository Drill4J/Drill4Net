using System;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Loggers;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Serilog;
using NLog;
using Drill4Net.BanderLog;
using Drill4Net.BanderLog.Sinks.File;
using System.Threading.Tasks;

namespace Drill4Net.Benchmark.Logger
{
    /// <summary>
    /// Benchmarks for comparison of different loggers
    /// </summary>
    [MemoryDiagnoser]
    [HtmlExporter]
    [Config(typeof(Config))]
    public class Tests
    {
        private List<string> _testData;
        private string _testString;
        [Params( 1000)]
        public int AmountOfStrings { get; set; }
        private NLog.Logger _loggerNlog;
        private BanderLog.Logger _loggerBanderLog;
        private string _fileName="LogFile.txt";
        private string _fileNameSeriLog = "LogFileSerilog.txt";
        private string _fileNameNLog = "LogFileNlog.txt";
        private string _fileNameBanderLog = "LogFileBanderLog.txt";
        
        private class Config : ManualConfig
        {

            public Config()
            {
                //AddLogger(ConsoleLogger.Default);
            }
        }
        [GlobalSetup]
        public void GlobalSetup()
        {
            _testString = new string('a', 1000);
            _testData = Enumerable.Repeat(_testString, AmountOfStrings).ToList();         
            Log.Logger=new LoggerConfiguration()
                .WriteTo.File(_fileNameSeriLog)
                .CreateLogger();
            _loggerNlog= LogManager.GetCurrentClassLogger();
            var logBld = new LogBuilder();
            _loggerBanderLog =logBld.AddSink(FileSinkCreator.CreateSink(_fileNameBanderLog)).Build();

        }

        [Benchmark]
        public void UseBanderLog()
        {
            for (var i = 0; i < AmountOfStrings; i++)
            {
                _loggerBanderLog.Log(Microsoft.Extensions.Logging.LogLevel.Information, _testString);
            }
        }
        [Benchmark]
        public void UseAppendAllLines()
        {
            File.AppendAllLines(_fileName, _testData);
        }
        [Benchmark]
        public void UseAppendAllText()
        {
            for (var i = 0; i < AmountOfStrings; i++)
            {
                File.AppendAllText(_fileName, _testString);
            }
        }
        [Benchmark]
        public void UseSerilog()
        {
            for(var i=0;i< AmountOfStrings; i++)
            {
                Log.Logger.Information(_testString);
            }
        }
        [Benchmark]
        public void UseWithNLog()
        {
            for (var i = 0; i < AmountOfStrings; i++)
            {
                _loggerNlog.Info(_testString);
            }
        }

        //[Benchmark]
        //public void WriteLogWithBanderLog5yTasks()
        //{
        //    Task[] tasks = new Task[5];
        //    for (var i = 0; i < 5; i++)
        //    {
        //        tasks[i] = Task.Run(() => WriteLogWithBanderLog());
        //    }

        //}


        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Log.CloseAndFlush();
            NLog.LogManager.Shutdown();
            _loggerBanderLog.Flush();
        }
    }

}
