using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using K4os.Compression.LZ4;
using Drill4Net.Common;
using Drill4Net.Compressor.Benchmarks.Enums;
using Drill4Net.Compressor.Benchmarks.Helpers;
using Drill4Net.Compressor.Benchmarks.src.Models;
using Drill4Net.Compressor.Benchmarks.Compression;

namespace Drill4Net.Compressor.Benchmarks
{
    internal class Tests
    {
        private IBenchmarkLogger  _fileLogger;
        private const string LOG_PATH = @"logs\benchmarkLog.txt";

        /**************************************************************************/
        internal Tests()
        {
            _fileLogger = new BenchmarkFileLogger(Path.Combine(FileUtils.ExecutingDir, LOG_PATH));
        }
        /**************************************************************************/

        internal TestResult DeflateTest(int iterations,byte[] data, CompressionLevel compressionLevel, DataTypes dataType)
        {
            double minTime, maxTime, avgTime, minCompressionRate, maxCompressionRate, avgCompressionRate, minMemory, maxMemory, avgMemory;
            minTime = maxTime = avgTime = minCompressionRate = maxCompressionRate = avgCompressionRate = minMemory = maxMemory = avgMemory = 0;

            for (var i=0; i<iterations;i++)
            {
                double memory = 0;
                byte[] compressed = null;
                var watcher = Stopwatch.StartNew();
                using (Process proc = Process.GetCurrentProcess())
                {
                    compressed = DeflateCompressor.CompressData(data, compressionLevel);
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                }
                watcher.Stop();
                var duration = watcher.Elapsed.TotalMilliseconds;
                double compressionRate = compressed.Length * 100 / data.Length;
                DataChecker.CounterChecker(ref duration, ref minTime, ref maxTime, ref avgTime);
                DataChecker.CounterChecker(ref compressionRate, ref minCompressionRate, ref maxCompressionRate, ref avgCompressionRate);
                DataChecker.CounterChecker(ref memory, ref minMemory, ref maxMemory, ref avgMemory);
            }

            avgTime = Math.Round(avgTime / iterations, 4);
            avgCompressionRate = avgCompressionRate / iterations;
            avgMemory = avgMemory / iterations;

            //var logMsg = $"DeflateTest {dataType} {compressionLevel} {minTime} {maxTime} {avgTime} {minTime} {maxTime} {avgTime} " +
            //    $"{minCompressionRate} {maxCompressionRate} {avgCompressionRate} {minMemory} {maxMemory} {avgMemory}";
            //BenchmarkLog.WriteBenchmarkToLog(_fileLogger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(), logMsg);
            //Console.WriteLine($"{dataType} CompressionLevel:{compressionLevel}{Environment.NewLine}" +
            //    $"minTime: {minTime} maxTime: {maxTime} avgTime: {avgTime}{Environment.NewLine}" +
            //    $"minCompressionRate: {minCompressionRate} maxCompressionRate: {maxCompressionRate} avgCompressionRate: {avgCompressionRate}{Environment.NewLine}" +
            //    $"minMemory: {minMemory} maxMemory: {maxMemory} avgMemory: {avgMemory}{Environment.NewLine}");

            return new TestResult()
            {
                MinTime = minTime,
                MaxTime = maxTime,
                AvgTime = avgTime,
                MinCompressionRate = minCompressionRate,
                MaxCompressionRate = maxCompressionRate,
                AvgCompressionRate = avgCompressionRate,
                MinMemory = minMemory,
                MaxMemory = maxCompressionRate,
                AvgMemory = avgMemory,
                CompressorName= "Deflate",
                CompressLevel=compressionLevel.ToString(),
                DataType= dataType
            };
        }

        internal TestResult LZ4Test(int iterations, byte[] data, LZ4Level compressionLevel, DataTypes dataType)
        {
            double minTime, maxTime, avgTime, minCompressionRate, maxCompressionRate, avgCompressionRate, minMemory, maxMemory, avgMemory;
            minTime = maxTime = avgTime = minCompressionRate = maxCompressionRate = avgCompressionRate = minMemory = maxMemory = avgMemory = 0;

            for (var i = 0; i < iterations; i++)
            {
                double memory = 0;
                byte[] compressed = null;
                var watcher = Stopwatch.StartNew();
                using (Process proc = Process.GetCurrentProcess())
                {
                    compressed = LZ4Compressor.CompressData(data, compressionLevel);
                    memory = proc.PrivateMemorySize64 / (1024 * 1024);
                }
                watcher.Stop();
                var duration = watcher.Elapsed.TotalMilliseconds;
                double compressionRate = compressed.Length * 100 / data.Length;
                DataChecker.CounterChecker(ref duration, ref minTime, ref maxTime, ref avgTime);
                DataChecker.CounterChecker(ref compressionRate, ref minCompressionRate, ref maxCompressionRate, ref avgCompressionRate);
                DataChecker.CounterChecker(ref memory, ref minMemory, ref maxMemory, ref avgMemory);
            }
            avgTime = avgTime / iterations;
            avgCompressionRate = avgCompressionRate / iterations;
            avgMemory = avgMemory / iterations;

            //var logMsg = $"LZ4Tests {dataType} {compressionLevel} {minTime} {maxTime} {avgTime} {minTime} {maxTime} {avgTime} " +
            //    $"{minCompressionRate} {maxCompressionRate} {avgCompressionRate} {minMemory} {maxMemory} {avgMemory}";
            //BenchmarkLog.WriteBenchmarkToLog(_fileLogger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(), logMsg);
            //Console.WriteLine($"{dataType} CompressionLevel:{compressionLevel}{Environment.NewLine}" +
            //    $"minTime: {minTime} maxTime: {maxTime} avgTime: {avgTime}{Environment.NewLine}" +
            //    $"minCompressionRate: {minCompressionRate} maxCompressionRate: {maxCompressionRate} avgCompressionRate: {avgCompressionRate}{Environment.NewLine}" +
            //    $"minMemory: {minMemory} maxMemory: {maxMemory} avgMemory: {avgMemory}{Environment.NewLine}");

            return new TestResult()
            {
                MinTime = minTime,
                MaxTime = maxTime,
                AvgTime = avgTime,
                MinCompressionRate = minCompressionRate,
                MaxCompressionRate = maxCompressionRate,
                AvgCompressionRate = avgCompressionRate,
                MinMemory = minMemory,
                MaxMemory = maxCompressionRate,
                AvgMemory = avgMemory,
                CompressorName = "LZ4",
                CompressLevel = compressionLevel.ToString(),
                DataType = dataType
            };
        }

    }
}
