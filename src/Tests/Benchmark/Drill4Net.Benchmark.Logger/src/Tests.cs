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
        private string _fileName="LogFile.txt";
        private LoggerConfiguration _seriLog;
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
            _testData = Enumerable.Repeat(new string('a', 5000), 100000).ToList();
            _fileName = @"C:\LogFile.txt";
            Log.Logger=
             new LoggerConfiguration()
    .WriteTo.File("log.txt")
    .CreateLogger();

            log.Information("Hello, Serilog!");
            //System.IO.File.Delete(fileName);
        }
        [Benchmark]
        public void WriteLogToFile()
        {
            File.AppendAllLines(_fileName, _testData);
        }
        [Benchmark]
        public void WriteLogSerilog()
        {
            var log = 
        }
    }

}
