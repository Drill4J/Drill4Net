﻿using System;
using System.Diagnostics;
using System.IO.Compression;
using K4os.Compression.LZ4;
using Drill4Net.Compressor.Benchmarks.Enums;
using Drill4Net.Compressor.Benchmarks.Models;
using Drill4Net.Compressor.Benchmarks.Helpers;
using Drill4Net.Compressor.Benchmarks.Compression;

namespace Drill4Net.Compressor.Benchmarks
{
    /// <summary>
    ///Tests for compressors
    /// <summary>
    internal class Tests
    {
        /// <summary>
        /// Test for Deflate Compressor
        /// </summary>
        /// <param name="iterations">Number of iterations</param>
        /// <param name="data">Data for compression</param>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="dataType"> Data Type (Simple, Comolex, etc.)</param>
        /// <returns>Test Result</returns>
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
        /// <summary>
        /// Test for LZ4 Compressor
        /// </summary>
        /// <param name="iterations">Number of iterations</param>
        /// <param name="data">Data for compression</param>
        /// <param name="compressionLevel">Compression Level</param>
        /// <param name="dataType"> Data Type (Simple, Comolex, etc.)</param>
        /// <returns>Test Result</returns>
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