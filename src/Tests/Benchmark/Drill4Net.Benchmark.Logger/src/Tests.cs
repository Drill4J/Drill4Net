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
        private int _amountOfStrings;
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
                AddLogger(ConsoleLogger.Default);
            }
        }
        [GlobalSetup]
        public void GlobalSetup()
        {
            _testString = new string('a', 5000);
            _testData = Enumerable.Repeat(_testString, _amountOfStrings).ToList();         
            Log.Logger=new LoggerConfiguration()
                .WriteTo.File(_fileNameSeriLog)
                .CreateLogger();
            _loggerNlog= LogManager.GetCurrentClassLogger();
            var logBld = new LogBuilder();
            _loggerBanderLog = logBld.CreateStandardLogger();

        }
        [Benchmark]
        public void WriteLogWithAppendAllLines()
        {
            File.AppendAllLines(_fileName, _testData);
        }
        [Benchmark]
        public void WriteLogWithAppendAllTextCycle()
        {
            for (var i = 0; i < _amountOfStrings; i++)
            {
                File.AppendAllText(_fileName, _testString);
            }
        }
        [Benchmark]
        public void WriteLogWithSerilogCycle()
        {
            for(var i=0;i< _amountOfStrings; i++)
            {
                Log.Logger.Information(_testString);
            }
        }
        [Benchmark]
        public void WriteLogWithSerilog()
        {
            Log.Logger.Information("{_testData}", _testData);
        }
        [Benchmark]
        public void WriteLogWithNLogCycle()
        {
            for (var i = 0; i < _amountOfStrings; i++)
            {
                _loggerNlog.Info(_testString);
            }
        }

        [Benchmark]
        public void WriteLogWithBanderLogCycle()
        {
            for (var i = 0; i < _amountOfStrings; i++)
            {
                _loggerBanderLog.Log(Microsoft.Extensions.Logging.LogLevel.Information, _testString);
            }
        }

        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Log.CloseAndFlush();
            NLog.LogManager.Shutdown();
            _loggerBanderLog.Flush();
        }
    }

}
