using System;
using System.Linq;
using System.IO.Compression;
using System.Collections.Generic;
using ConsoleTables;
using K4os.Compression.LZ4;
using Drill4Net.Common;
using Drill4Net.Compressor.Benchmarks.Models;
using Conf=Drill4Net.Compressor.Benchmarks.CompressorConfig;

namespace Drill4Net.Compressor.Benchmarks.Helpers
{
    internal static class ResultReporter
    {
        /// <summary>
        /// Test for Deflate Compressor
        /// </summary>
        /// <param name="testResults">Data for processing</param>
        /// <param name="logger">Logger</param>
        internal static void PrintAndLogResult(List<TestResult> testResults, IBenchmarkLogger logger)
        {
            Console.WriteLine($"{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}" +
                $"Compressor Test Results{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}");

            var resultsOrdered = testResults.OrderBy(c => c.CompressorType).ThenBy(l => l.CompressLevel).ThenBy(t => t.DataType).ToList();

                //Tables with absolute result
                var tableAbsTime = new ConsoleTable(Conf.COMPRESSOR, Conf.COMPRESSOR_LEVEL, Conf.DATA_TYPE, Conf.MIN_TIME, Conf.MAX_TIME, Conf.AVG_TIME);
                var tableAbsCompress = new ConsoleTable(Conf.COMPRESSOR, Conf.COMPRESSOR_LEVEL, Conf.DATA_TYPE, Conf.MIN_COMR_RATE, Conf.MAX_COMR_RATE, Conf.AVG_COMR_RATE);
                var tableAbsMemory = new ConsoleTable(Conf.COMPRESSOR, Conf.COMPRESSOR_LEVEL, Conf.DATA_TYPE, Conf.MIN_MEMORY_USAGE, Conf.MAX_MEMORY_USAGE, Conf.AVG_MEMORY_USAGE);

                foreach (var result in resultsOrdered)
                {
                    tableAbsTime.AddRow(result.CompressorType, result.CompressLevel, result.DataType, result.MinTime, result.MaxTime, Math.Round(result.AvgTime, 4));
                    tableAbsCompress.AddRow(result.CompressorType, result.CompressLevel, result.DataType, result.MinCompressionRate, result.MaxCompressionRate, Math.Round(result.AvgCompressionRate, 4));
                    tableAbsMemory.AddRow(result.CompressorType, result.CompressLevel, result.DataType, result.MinMemory, result.MaxMemory, Math.Round(result.AvgMemory, 4));

                    //log
                    var logMsg = $"{result.CompressorType} { result.CompressLevel} {result.DataType} {result.MinTime} {result.MaxTime} {Math.Round(result.AvgTime, 4)} " +
                        $"{result.MinCompressionRate} {result.MaxCompressionRate} {Math.Round(result.AvgCompressionRate, 4)} " +
                            $"{result.MinMemory} {result.MaxMemory} {Math.Round(result.AvgMemory, 4)}";
                    BenchmarkLog.WriteBenchmarkToLog(logger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(), logMsg);
                }

                Console.WriteLine($"{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}" +
                    $"Absolute results for Compression time{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}");
                tableAbsTime.Write();

                Console.WriteLine($"{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}" +
                    $"Absolute results for Compression Rate{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}");
                tableAbsCompress.Write();

                Console.WriteLine($"{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}" +
                    $"Absolute results for Compression Memory Usage{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}");
                tableAbsMemory.Write();

            //Tables with Relative result
            Console.WriteLine($"{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}" +
               $"Relative results for Compressors (LZ4 in comparison with Deflate){Environment.NewLine}{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}");
            Console.WriteLine($"{Environment.NewLine}Calculated as LZ4_Value/Deflate_Value {Environment.NewLine}" +
                $"The lower the value, the better the LZ4 compressor performance {Environment.NewLine}" +
                $"Value more then 1 means that Deflate value is better than LZ4 value  {Environment.NewLine}" +
                    $"Value less then 1 means that LZ4 value is better than Deflate value  {Environment.NewLine}" +
                        $"Value equal to 1 means that LZ4 value is equal to Deflate value  {Environment.NewLine}");

            var tableRelative = new ConsoleTable("Comression Level LZ4", "To Comression Level Deflate", Conf.DATA_TYPE, "AvgTime", "AvgCompressionRate", "AvgMemoryUsage");
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L03_HC.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L03_HC.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L03_HC.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L10_OPT.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L10_OPT.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L10_OPT.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L09_HC.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L09_HC.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L09_HC.ToString(), CompressionLevel.Optimal.ToString(), ModelTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L00_FAST.ToString(), CompressionLevel.Fastest.ToString(), ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L00_FAST.ToString(), CompressionLevel.Fastest.ToString(), ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, LZ4Level.L00_FAST.ToString(), CompressionLevel.Fastest.ToString(), ModelTypes.InjectedSolution);
            tableRelative.Write();

            //Tables with Range results            
            var tableRange = new ConsoleTable("Indicator", Conf.COMPRESSOR, Conf.DATA_TYPE, "Min", "Min Level", "Max", "Max Level", "Range");

            foreach (ModelTypes dataType in Enum.GetValues(typeof(ModelTypes)))
            {
                AddToRangeTable(ref tableRange, resultsOrdered, CompressorTypes.LZ4, dataType);
                AddToRangeTable(ref tableRange, resultsOrdered, CompressorTypes.Deflate, dataType);
            }
            Console.WriteLine($"{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}" +
               $"Range results{Environment.NewLine}{Conf.SEPARATOR}{Environment.NewLine}");
            Console.WriteLine($"{Environment.NewLine}Calculated as Max/Min {Environment.NewLine}" +
               $"The lower the value, the narrower the range {Environment.NewLine}");
           tableRange.Write();
        }

        private static void AddToRelativeTable(ref ConsoleTable table, List<TestResult> results, string lz4ComprLevel, string deflateComprLevel, ModelTypes dataType)
        {
            var lz4 = results.Single(r => r.CompressorType == CompressorTypes.LZ4 && r.CompressLevel == lz4ComprLevel && r.DataType == dataType);
            var deflate = results.Single(r => r.CompressorType == CompressorTypes.Deflate && r.CompressLevel == deflateComprLevel && r.DataType == dataType);

                var time = Math.Round(lz4.AvgTime / deflate.AvgTime, 2);
                var rate = Math.Round(lz4.AvgCompressionRate/ deflate.AvgCompressionRate, 2);
                var memory = Math.Round(lz4.AvgMemory/ deflate.AvgMemory, 2);
                table.AddRow(lz4ComprLevel, deflateComprLevel, dataType, time, rate, memory);
        }

        private static void AddRowToRangeTable(ref ConsoleTable table, string indicator, CompressorTypes compressor, ModelTypes dataType, double min, double max, string minCompressLevel, string maxCompressLevel)
        {
            table.AddRow(indicator, compressor, dataType, Math.Round(min,2), minCompressLevel == maxCompressLevel ? Conf.ALL_TESTED : minCompressLevel,
                    Math.Round(max,2), maxCompressLevel == minCompressLevel ? Conf.ALL_TESTED : maxCompressLevel, Math.Round((max/min),2));
        }

        private static void AddToRangeTable(ref ConsoleTable table,List<TestResult> testResults, CompressorTypes compressor, ModelTypes dataType)
        {
            var maxTime = testResults.Where(rd => rd.DataType == dataType && rd.CompressorType == compressor).OrderByDescending(r => r.AvgTime).FirstOrDefault();
            var minTime = testResults.Where(rd => rd.DataType == dataType && rd.CompressorType == compressor).OrderBy(r => r.AvgTime).FirstOrDefault();
            var maxRate = testResults.Where(rd => rd.DataType == dataType && rd.CompressorType == compressor).OrderByDescending(r => r.AvgCompressionRate).FirstOrDefault();
            var minRate = testResults.Where(rd => rd.DataType == dataType && rd.CompressorType == compressor).OrderBy(r => r.AvgCompressionRate).FirstOrDefault();
            var maxMemory = testResults.Where(rd => rd.DataType == dataType && rd.CompressorType == compressor).OrderByDescending(r => r.AvgMemory).FirstOrDefault();
            var minMemory = testResults.Where(rd => rd.DataType == dataType && rd.CompressorType == compressor).OrderBy(r => r.AvgMemory).FirstOrDefault();
            AddRowToRangeTable(ref table, Conf.AVG_TIME, compressor, dataType, minTime.MinTime, maxTime.MaxTime,minTime.CompressLevel, maxTime.CompressLevel);
            AddRowToRangeTable(ref table, Conf.AVG_COMR_RATE, compressor, dataType, minRate.MinCompressionRate, maxRate.MaxCompressionRate, minRate.CompressLevel, maxRate.CompressLevel);
        }
    }
}
