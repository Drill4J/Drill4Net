using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;
using K4os.Compression.LZ4;
using Drill4Net.Common;
using Drill4Net.Compressor.Benchmarks.Enums;
using Drill4Net.Compressor.Benchmarks.Models;
using Drill4Net.Compressor.Benchmarks.Helpers;
using Drill4Net.Profiling.Tree;
using Drill4Net.Compressor.Benchmarks.src.Models;
using ConsoleTables;
using System.Linq;

namespace Drill4Net.Compressor.Benchmarks
{
    class Program
    {
        const string CFG_NAME= "inj_Std.yml";
        const int ITERATIONS = 10;
        const int ITERATIONS_INJ = 10;
        static async Task Main(string[] args)
        {
            var tests = new Tests();
            var cfgPath = Path.Combine(FileUtils.ExecutingDir, CFG_NAME);
            var simpleData = PrepareData.GenerateSimpleData();
            var simpleDataBytes = Serializer.ToArray(simpleData);
            var mediumData = PrepareData.GenerateMediumData();
            var mediumDataBytes = Serializer.ToArray(mediumData);
            var complexData = PrepareData.GenerateComplexData();
            var complexDataBytes = Serializer.ToArray<ComplexData>(complexData);
            var injectedData = await PrepareData.GenerateInjectedSolutionAsync(cfgPath);
            var injectedDataBytes = Serializer.ToArray<InjectedSolution>(injectedData);
            var testResults = new List<TestResult>();

            testResults.Add(tests.DeflateTest(ITERATIONS, simpleDataBytes, CompressionLevel.Optimal, DataTypes.Simple));
            testResults.Add(tests.DeflateTest(ITERATIONS, simpleDataBytes, CompressionLevel.Fastest, DataTypes.Simple));

            testResults.Add(tests.DeflateTest(ITERATIONS, mediumDataBytes, CompressionLevel.Optimal, DataTypes.Medium));
            testResults.Add(tests.DeflateTest(ITERATIONS, mediumDataBytes, CompressionLevel.Fastest, DataTypes.Medium));

            testResults.Add(tests.DeflateTest(ITERATIONS, complexDataBytes, CompressionLevel.Optimal, DataTypes.Complex));
            testResults.Add(tests.DeflateTest(ITERATIONS, complexDataBytes, CompressionLevel.Fastest, DataTypes.Complex));

            testResults.Add(tests.DeflateTest(ITERATIONS_INJ, injectedDataBytes, CompressionLevel.Optimal, DataTypes.InjectedSolution));
            testResults.Add(tests.DeflateTest(ITERATIONS_INJ, injectedDataBytes, CompressionLevel.Fastest, DataTypes.InjectedSolution));

            testResults.Add(tests.LZ4Test(ITERATIONS, simpleDataBytes, LZ4Level.L03_HC, DataTypes.Simple));
            testResults.Add(tests.LZ4Test(ITERATIONS, simpleDataBytes, LZ4Level.L10_OPT, DataTypes.Simple));
            testResults.Add(tests.LZ4Test(ITERATIONS, simpleDataBytes, LZ4Level.L00_FAST, DataTypes.Simple));
            testResults.Add(tests.LZ4Test(ITERATIONS, simpleDataBytes, LZ4Level.L09_HC, DataTypes.Simple));

            testResults.Add(tests.LZ4Test(ITERATIONS, mediumDataBytes, LZ4Level.L03_HC, DataTypes.Medium));
            testResults.Add(tests.LZ4Test(ITERATIONS, mediumDataBytes, LZ4Level.L10_OPT, DataTypes.Medium));
            testResults.Add(tests.LZ4Test(ITERATIONS, mediumDataBytes, LZ4Level.L00_FAST, DataTypes.Medium));
            testResults.Add(tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L09_HC, DataTypes.Medium));

            testResults.Add(tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L03_HC, DataTypes.Complex));
            testResults.Add(tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L10_OPT, DataTypes.Complex));
            testResults.Add(tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L00_FAST, DataTypes.Complex));
            testResults.Add(tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L09_HC, DataTypes.Complex));

            testResults.Add(tests.LZ4Test(ITERATIONS_INJ, injectedDataBytes, LZ4Level.L03_HC, DataTypes.InjectedSolution));
            testResults.Add(tests.LZ4Test(ITERATIONS_INJ, injectedDataBytes, LZ4Level.L10_OPT, DataTypes.InjectedSolution));
            testResults.Add(tests.LZ4Test(ITERATIONS_INJ, injectedDataBytes, LZ4Level.L00_FAST, DataTypes.InjectedSolution));
            testResults.Add(tests.LZ4Test(ITERATIONS_INJ, injectedDataBytes, LZ4Level.L09_HC, DataTypes.InjectedSolution));

            PrintResult(testResults);


        }
        static void PrintResult(List<TestResult> testResults)
        {
            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
                $"Compressor Test Results{Environment.NewLine}***************************{Environment.NewLine}");

            var resultsOrdered = testResults.OrderBy(c => c.CompressorName).ThenBy(l => l.CompressLevel).ThenBy(t => t.DataType).ToList();
            var tableAbsTime = new ConsoleTable("Comression Level", "Compressor", "Data Type", "MinTime(msec)", "MaxTime(msec)", "AvgTime(msec)");
            var tableAbsCompress = new ConsoleTable("Comression Level", "Compressor", "Data Type", "MinCompressionRate(%)", "MaxCompressionRate(%)", "AvgCompressionRate(%)");
            var tableAbsMemory = new ConsoleTable("Comression Level", "Compressor", "Data Type", "MinMemoryUsage(MByte)", "MaxMemoryUsage(MByte)", "AvgMemoryUsage(MByte)");
            
            foreach (var result in resultsOrdered)
            {
                tableAbsTime.AddRow(result.CompressLevel, result.CompressorName, result.DataType, result.MinTime, result.MaxTime, Math.Round(result.AvgTime, 4));
                tableAbsCompress.AddRow(result.CompressLevel, result.CompressorName, result.DataType, result.MinCompressionRate, result.MaxCompressionRate, Math.Round(result.AvgCompressionRate, 4));
                tableAbsMemory.AddRow(result.CompressLevel, result.CompressorName, result.DataType, result.MinMemory, result.MaxMemory, Math.Round(result.AvgMemory, 4));
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

            
            var tableRelative = new ConsoleTable();
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L03_HC", "Optimal", DataTypes.Medium, false);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L03_HC", "Optimal", DataTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L03_HC", "Optimal", DataTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L10_OPT", "Optimal", DataTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L10_OPT", "Optimal", DataTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L10_OPT", "Optimal", DataTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L09_HC", "Optimal", DataTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L09_HC", "Optimal", DataTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L09_HC", "Optimal", DataTypes.InjectedSolution);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L00_FAST", "Fastest", DataTypes.Medium);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L00_FAST", "Fastest", DataTypes.Complex);
            AddToRelativeTable(ref tableRelative, resultsOrdered, "L00_FAST", "Fastest", DataTypes.InjectedSolution);

            tableRelative.Write();

        }

        static void AddToRelativeTable(ref ConsoleTable table, List<TestResult> results, string lz4ComprLevel, string deflateComprLevel, DataTypes dataType, bool skipHeader=true )
        {
            if(! skipHeader)
            {
                var names = new List<string> { "Comression Level LZ4", "To Comression Level Deflate", "Data Type", "AvgTime(%)", "AvgCompressionRate(%)", "AvgMemoryUsage(%)" };
                table.AddColumn(names);
            }
            var lz4 = results.Where(r => r.CompressorName == "LZ4" && r.CompressLevel == lz4ComprLevel && r.DataType == dataType).FirstOrDefault();
            var deflate = results.Where(r => r.CompressorName == "Deflate" && r.CompressLevel == deflateComprLevel && r.DataType == dataType).FirstOrDefault();
            
            if (lz4!=null && deflate!=null)
            {
                var time = Math.Round(((lz4.AvgTime - deflate.AvgTime) / deflate.AvgTime) * 100,2);
                var rate = Math.Round(((lz4.AvgCompressionRate - deflate.AvgCompressionRate) / deflate.AvgCompressionRate) * 100,2);
                var memory = Math.Round(((lz4.AvgMemory - deflate.AvgMemory) / deflate.AvgMemory) * 100, 2);
                table.AddRow(lz4ComprLevel, deflateComprLevel, dataType, time, rate, memory);
            }
        }
    }
}
