using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using K4os.Compression.LZ4;
using Drill4Net.Common;
using Drill4Net.Compressor.Benchmarks.Enums;
using Drill4Net.Compressor.Benchmarks.Helpers;
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

        internal void DeflateTest(int iterations,byte[] data, CompressionLevel compressionLevel, DataTypes dataType)
        {
            double minTime, maxTime, avgTime, minCompressionRate, maxCompressionRate, avgCompressionRate;
            minTime = maxTime = avgTime = minCompressionRate = maxCompressionRate = avgCompressionRate = 0;

            for (var i=0; i<iterations;i++)
            {
                var watcher = Stopwatch.StartNew();
                var compressed = DeflateCompressor.CompressData(data, compressionLevel);
                watcher.Stop();
                var duration = watcher.Elapsed.TotalMilliseconds;
                double compressionRate = compressed.Length * 100 / data.Length;
                DataChecker.CounterChecker(ref duration, ref minTime, ref maxTime, ref avgTime);
                DataChecker.CounterChecker(ref compressionRate, ref minCompressionRate, ref maxCompressionRate, ref avgCompressionRate);
            }

            avgTime = Math.Round(avgTime / iterations, 4);
            avgCompressionRate = avgCompressionRate / iterations;
            var logMsg = $"DeflateTest {dataType} {compressionLevel} {minTime} {maxTime} {avgTime} {minTime} {maxTime} {avgTime} " +
                $"{minCompressionRate} {maxCompressionRate} {avgCompressionRate}";
            BenchmarkLog.WriteBenchmarkToLog(_fileLogger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(), logMsg);
            Console.WriteLine($"{dataType} CompressionLevel:{compressionLevel}{Environment.NewLine}" +
                $"minTime: {minTime} maxTime: {maxTime} avgTime: {avgTime}{Environment.NewLine}" +
                $"minCompressionRate: {minCompressionRate} maxCompressionRate: {maxCompressionRate} avgCompressionRate: {avgCompressionRate}{Environment.NewLine}");
        }

        internal void LZ4Test(int iterations, byte[] data, LZ4Level compressionLevel, DataTypes dataType)
        {
            double minTime, maxTime, avgTime, minCompressionRate, maxCompressionRate, avgCompressionRate;
            minTime = maxTime = avgTime = minCompressionRate = maxCompressionRate = avgCompressionRate = 0;
            for (var i = 0; i < iterations; i++)
            {
                var watcher = Stopwatch.StartNew();
                var compressed = LZ4Compressor.CompressData(data, compressionLevel);
                watcher.Stop();
                var duration = watcher.Elapsed.TotalMilliseconds;
                double compressionRate = compressed.Length * 100 / data.Length;
                DataChecker.CounterChecker(ref duration, ref minTime, ref maxTime, ref avgTime);
                DataChecker.CounterChecker(ref compressionRate, ref minCompressionRate, ref maxCompressionRate, ref avgCompressionRate);
            }
            avgTime =Math.Round( avgTime / iterations, 4);
            avgCompressionRate = avgCompressionRate / iterations;
            var logMsg = $"DeflateTest {dataType} {compressionLevel} {minTime} {maxTime} {avgTime} {minTime} {maxTime} {avgTime} " +
                $"{minCompressionRate} {maxCompressionRate} {avgCompressionRate}";
            BenchmarkLog.WriteBenchmarkToLog(_fileLogger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(), logMsg);
            Console.WriteLine($"{dataType} CompressionLevel:{compressionLevel}{Environment.NewLine}" +
                $"minTime: {minTime} maxTime: {maxTime} avgTime: {avgTime}{Environment.NewLine}" +
                $"minCompressionRate: {minCompressionRate} maxCompressionRate: {maxCompressionRate} avgCompressionRate: {avgCompressionRate}{Environment.NewLine}");
        }

    }
}
