using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using K4os.Compression.LZ4;
using Drill4Net.Common;
using Drill4Net.Compressor.Benchmarks.Dto;
using Drill4Net.Compressor.Benchmarks.Enums;
using Drill4Net.Compressor.Benchmarks.Helpers;
using Drill4Net.Profiling.Tree;

namespace Drill4Net.Compressor.Benchmarks
{
    class Program
    {
        const string CFG_NAME= "inj_Std.yml";
        const int ITERATIONS = 1000;
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

            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
                $"Compressor Benchmark Test{Environment.NewLine}***************************{Environment.NewLine}" +
                $"{Environment.NewLine}DeflateCompressor Test{Environment.NewLine}" +
                $"***************************{Environment.NewLine}");
            tests.DeflateTest(ITERATIONS, simpleDataBytes, CompressionLevel.Optimal, DataTypes.Simple);
            tests.DeflateTest(ITERATIONS, simpleDataBytes, CompressionLevel.NoCompression, DataTypes.Simple);
            tests.DeflateTest(ITERATIONS, simpleDataBytes, CompressionLevel.Fastest, DataTypes.Simple);

            tests.DeflateTest(ITERATIONS, mediumDataBytes, CompressionLevel.Optimal, DataTypes.Medium);
            tests.DeflateTest(ITERATIONS, mediumDataBytes, CompressionLevel.NoCompression, DataTypes.Medium);
            tests.DeflateTest(ITERATIONS, mediumDataBytes, CompressionLevel.Fastest, DataTypes.Medium);

            tests.DeflateTest(ITERATIONS, complexDataBytes, CompressionLevel.Optimal, DataTypes.Complex);
            tests.DeflateTest(ITERATIONS, complexDataBytes, CompressionLevel.NoCompression, DataTypes.Complex);
            tests.DeflateTest(ITERATIONS, complexDataBytes, CompressionLevel.Fastest, DataTypes.Complex);

            tests.DeflateTest(100, injectedDataBytes, CompressionLevel.Optimal, DataTypes.InjectedSolution);
            tests.DeflateTest(100, injectedDataBytes, CompressionLevel.NoCompression, DataTypes.InjectedSolution);
            tests.DeflateTest(100, injectedDataBytes, CompressionLevel.Fastest, DataTypes.InjectedSolution);
            Console.WriteLine($"{Environment.NewLine}***************************{Environment.NewLine}" +
                $"{Environment.NewLine}LZ4Compressor Test{Environment.NewLine}" +
                $"***************************{Environment.NewLine}");
            tests.LZ4Test(ITERATIONS, simpleDataBytes, LZ4Level.L03_HC, DataTypes.Simple);
            tests.LZ4Test(ITERATIONS, simpleDataBytes, LZ4Level.L10_OPT, DataTypes.Simple);
            tests.LZ4Test(ITERATIONS, simpleDataBytes, LZ4Level.L00_FAST, DataTypes.Simple);

            tests.LZ4Test(ITERATIONS, mediumDataBytes, LZ4Level.L03_HC, DataTypes.Medium);
            tests.LZ4Test(ITERATIONS, mediumDataBytes, LZ4Level.L10_OPT, DataTypes.Medium);
            tests.LZ4Test(ITERATIONS, mediumDataBytes, LZ4Level.L00_FAST, DataTypes.Medium);

            tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L03_HC, DataTypes.Complex);
            tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L10_OPT, DataTypes.Complex);
            tests.LZ4Test(ITERATIONS, complexDataBytes, LZ4Level.L00_FAST, DataTypes.Complex);

            tests.LZ4Test(100, injectedDataBytes, LZ4Level.L03_HC, DataTypes.InjectedSolution);
            tests.LZ4Test(100, injectedDataBytes, LZ4Level.L10_OPT, DataTypes.InjectedSolution);
            tests.LZ4Test(100, injectedDataBytes, LZ4Level.L00_FAST, DataTypes.InjectedSolution);
        }
    }
}
