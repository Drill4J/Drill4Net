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

namespace Drill4Net.Benchmark.Logger
{
    [MemoryDiagnoser]
    [HtmlExporter]
    [Config(typeof(Config))]
    public class Tests
    {
        private List<string> _testData;
        private string _testString;
        private int _amountOfStrings;
        private string _fileName="LogFile.txt";
        private string _fileNameSeriLog = "LogFileSerilog.txt";
        private string _fileNameNLog = "LogFileNlog.txt";
        private string _fileNameBanderLog = "LogFileBanderLog.txt";
        //private Serilog.ILogger _seriLog;
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
        }
        [Benchmark]
        public void WriteLogToFile()
        {
            File.AppendAllLines(_fileName, _testData);
        }
        [Benchmark]
        public void WriteLogSerilogForEach()
        {
            for(var i=0;i< _amountOfStrings; i++)
            {
                Log.Logger.Information(_testString);
            }
        }
        [Benchmark]
        public void WriteLogSerilog()
        {
            Log.Logger.Information("{_testData}", _testData);
        }
        [Benchmark]
        public void WriteLogNLog()
        {
           
        }
        [GlobalCleanup]
        public void GlobalCleanUp()
        {
            Log.CloseAndFlush();
        }
    }

}
