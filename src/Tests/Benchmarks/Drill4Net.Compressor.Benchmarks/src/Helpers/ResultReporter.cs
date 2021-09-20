using System;
using System.Linq;
using System.Collections.Generic;
using ConsoleTables;
using Drill4Net.Common;
using Drill4Net.Compressor.Benchmarks.Models;

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
            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
                $"Compressor Test Results{Environment.NewLine}***************************{Environment.NewLine}");

            //Tables with absolute result
            var resultsOrdered = testResults.OrderBy(c => c.CompressorName).ThenBy(l => l.CompressLevel).ThenBy(t => t.DataType).ToList();
            var tableAbsTime = new ConsoleTable("Compressor", "Comression Level", "Data Type", "MinTime(msec)", "MaxTime(msec)", "AvgTime(msec)");
            var tableAbsCompress = new ConsoleTable("Comression Level", "Compressor", "Data Type", "MinCompressionRate(%)", "MaxCompressionRate(%)", "AvgCompressionRate(%)");
            var tableAbsMemory = new ConsoleTable("Comression Level", "Compressor", "Data Type", "MinMemoryUsage(MByte)", "MaxMemoryUsage(MByte)", "AvgMemoryUsage(MByte)");

            foreach (var result in resultsOrdered)
            {
                tableAbsTime.AddRow(result.CompressorName, result.CompressLevel, result.DataType, result.MinTime, result.MaxTime, Math.Round(result.AvgTime, 4));
                tableAbsCompress.AddRow(result.CompressorName, result.CompressLevel, result.DataType, result.MinCompressionRate, result.MaxCompressionRate, Math.Round(result.AvgCompressionRate, 4));
                tableAbsMemory.AddRow(result.CompressorName, result.CompressLevel, result.DataType, result.MinMemory, result.MaxMemory, Math.Round(result.AvgMemory, 4));
                
                //log
                var logMsg = $"{result.CompressorName} { result.CompressLevel} {result.DataType} {result.MinTime} {result.MaxTime} {Math.Round(result.AvgTime, 4)} " +
                    $"{result.MinCompressionRate} {result.MaxCompressionRate} {Math.Round(result.AvgCompressionRate, 4)} " +
                        $"{result.MinMemory} {result.MaxMemory} {Math.Round(result.AvgMemory, 4)}";
                BenchmarkLog.WriteBenchmarkToLog(logger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(), logMsg);
            }

            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
                $"Absolute results for Compression time{Environment.NewLine}***************************{Environment.NewLine}");
            tableAbsTime.Write();
            
            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
                $"Absolute results for Compression Rate{Environment.NewLine}***************************{Environment.NewLine}");
            tableAbsCompress.Write();
            
            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
                $"Absolute results for Compression Memory Usage{Environment.NewLine}***************************{Environment.NewLine}");
            tableAbsMemory.Write();

            //Tables with Relative result
            Console.WriteLine($"{Environment.NewLine}**********************************************************{Environment.NewLine}" +
               $"Relative results for Compressors (LZ4 in comparison with Deflate){Environment.NewLine}**********************************************************{Environment.NewLine}");
            
            var tableRelative = new ConsoleTable("Comression Level LZ4", "To Comression Level Deflate", "Data Type", "AvgTime(%)", "AvgCompressionRate(%)", "AvgMemoryUsage(%)");
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L03_HC", "Optimal", ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L03_HC", "Optimal", ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L03_HC", "Optimal", ModelTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L10_OPT", "Optimal", ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L10_OPT", "Optimal", ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L10_OPT", "Optimal", ModelTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L09_HC", "Optimal", ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L09_HC", "Optimal", ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L09_HC", "Optimal", ModelTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L00_FAST", "Fastest", ModelTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L00_FAST", "Fastest", ModelTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L00_FAST", "Fastest", ModelTypes.InjectedSolution);
            tableRelative.Write();

            //Tables with Range results
            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
               $"Range results{Environment.NewLine}***************************{Environment.NewLine}");
            var tableRange = CreateRangeTable(testResults);
            tableRange.Write();
        }

        private static void AddToRelativeTable(ref ConsoleTable table, List<TestResult> results, string lz4ComprLevel, string deflateComprLevel, ModelTypes dataType)
        {
            var lz4 = results.Single(r => r.CompressorName == "LZ4" && r.CompressLevel == lz4ComprLevel && r.DataType == dataType);
            var deflate = results.Single(r => r.CompressorName == "Deflate" && r.CompressLevel == deflateComprLevel && r.DataType == dataType);

            if (lz4 != null && deflate != null)
            {
                var time = Math.Round(((lz4.AvgTime - deflate.AvgTime) / deflate.AvgTime) * 100, 2);
                var rate = Math.Round(((lz4.AvgCompressionRate - deflate.AvgCompressionRate) / deflate.AvgCompressionRate) * 100, 2);
                var memory = Math.Round(((lz4.AvgMemory - deflate.AvgMemory) / deflate.AvgMemory) * 100, 2);
                table.AddRow(lz4ComprLevel, deflateComprLevel, dataType, time, rate, memory);
            }
        }

        private static ConsoleTable CreateRangeTable(List<TestResult> testResults)
        {
            var tableRange = new ConsoleTable("Indicator", "Compressor", "Data Type", "Min", "Min Level", "Max", "Max Level", "Range");

            foreach (ModelTypes dataType in Enum.GetValues(typeof(ModelTypes)))
            {
                var lz4MaxTime = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "LZ4").OrderByDescending(r => r.AvgTime).FirstOrDefault();
                var lz4MinTime = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "LZ4").OrderBy(r => r.AvgTime).FirstOrDefault();
                var lz4MaxRate = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "LZ4").OrderByDescending(r => r.AvgCompressionRate).FirstOrDefault();
                var lz4MinRate = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "LZ4").OrderBy(r => r.AvgCompressionRate).FirstOrDefault();
                var lz4MaxMemory = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "LZ4").OrderByDescending(r => r.AvgMemory).FirstOrDefault();
                var lz4MinMemory = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "LZ4").OrderBy(r => r.AvgMemory).FirstOrDefault();

                var deflateMaxTime = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "Deflate").OrderByDescending(r => r.AvgTime).FirstOrDefault();
                var deflateMinTime = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "Deflate").OrderBy(r => r.AvgTime).FirstOrDefault();
                var deflateMaxRate = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "Deflate").OrderByDescending(r => r.AvgCompressionRate).FirstOrDefault();
                var deflateMinRate = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "Deflate").OrderBy(r => r.AvgCompressionRate).FirstOrDefault();
                var deflateMaxMemory = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "Deflate").OrderByDescending(r => r.AvgMemory).FirstOrDefault();
                var deflateMinMemory = testResults.Where(rd => rd.DataType == dataType && rd.CompressorName == "Deflate").OrderBy(r => r.AvgMemory).FirstOrDefault();
                var tableRangeLZ4 = new ConsoleTable("Indicator", "LZ4 Min", "LZ4 Min Level", "LZ4 Max", "LZ4 Max Level", "LZ4 Range");
                
                //LZ4 rows
                tableRange.AddRow("AvgTime (msec)", "LZ4", dataType, Math.Round(lz4MinTime.AvgTime), lz4MinTime.CompressLevel == lz4MaxTime.CompressLevel ? "All Tested" : lz4MinTime.CompressLevel,
                    Math.Round(lz4MaxTime.AvgTime), lz4MaxTime.CompressLevel == lz4MinTime.CompressLevel ? "All Tested" : lz4MaxTime.CompressLevel, Math.Round(lz4MaxTime.AvgTime - lz4MinTime.AvgTime));
                tableRange.AddRow("AvgCompressionRate (%)", "LZ4", dataType, Math.Round(lz4MinRate.AvgCompressionRate), lz4MinRate.CompressLevel == lz4MaxRate.CompressLevel ? "All Tested" : lz4MinRate.CompressLevel,
                    Math.Round(lz4MaxRate.AvgCompressionRate), lz4MaxRate.CompressLevel == lz4MinRate.CompressLevel ? "All Tested" : lz4MaxRate.CompressLevel, Math.Round(lz4MaxRate.AvgCompressionRate - lz4MinRate.AvgCompressionRate));
                tableRange.AddRow("AvgMemoryUsage (Mbyte)", "LZ4", dataType, Math.Round(lz4MinMemory.AvgMemory), lz4MinMemory.CompressLevel == lz4MaxMemory.CompressLevel ? "All Tested" : lz4MinMemory.CompressLevel,
                    Math.Round(lz4MaxMemory.AvgMemory), lz4MaxMemory.CompressLevel == lz4MinMemory.CompressLevel ? "All Tested" : lz4MaxMemory.CompressLevel, Math.Round(lz4MaxMemory.AvgMemory - lz4MinMemory.AvgMemory));
                
                //Deflate rows
                tableRange.AddRow("AvgTime (msec)", "Deflate", dataType, Math.Round(deflateMinTime.AvgTime), deflateMinTime.CompressLevel == deflateMaxTime.CompressLevel ? "All Tested" : deflateMinTime.CompressLevel,
                   Math.Round(deflateMaxTime.AvgTime), deflateMaxTime.CompressLevel == deflateMinTime.CompressLevel ? "All Tested" : deflateMaxTime.CompressLevel, Math.Round(deflateMaxTime.AvgTime - deflateMinTime.AvgTime));
                tableRange.AddRow("AvgCompressionRate (%)", "Deflate", dataType, Math.Round(deflateMinRate.AvgCompressionRate), deflateMinRate.CompressLevel == deflateMaxRate.CompressLevel ? "All Tested" : deflateMinRate.CompressLevel,
                    Math.Round(deflateMaxRate.AvgCompressionRate), deflateMaxRate.CompressLevel == deflateMinRate.CompressLevel ? "All Tested" : deflateMaxRate.CompressLevel, Math.Round(deflateMaxRate.AvgCompressionRate - deflateMinRate.AvgCompressionRate));
                tableRange.AddRow("AvgMemoryUsage (MByte)", "Deflate", dataType, Math.Round(deflateMinMemory.AvgMemory), deflateMinMemory.CompressLevel == deflateMaxMemory.CompressLevel ? "All Tested" : deflateMinMemory.CompressLevel,
                    Math.Round(deflateMaxMemory.AvgMemory), deflateMaxMemory.CompressLevel == deflateMinMemory.CompressLevel ? "All Tested" : deflateMaxMemory.CompressLevel, Math.Round(deflateMaxMemory.AvgMemory - deflateMinMemory.AvgMemory));
            }
            return tableRange;
        }
    }
}
