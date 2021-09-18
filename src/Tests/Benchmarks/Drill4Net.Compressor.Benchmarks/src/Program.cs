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

namespace Drill4Net.Compressor.Benchmarks
{
    /// <summary>
    ///Run Tests
    /// <summary>
    class Program
    {
/*******************************************************/
        const string CFG_NAME= "inj_Std.yml";
        const int ITERATIONS = 1000;
        const int ITERATIONS_INJ = 100;
        private const string LOG_PATH = @"logs\benchmarkLog.txt";
/*******************************************************/
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
            IBenchmarkLogger fileLogger= new BenchmarkFileLogger(Path.Combine(FileUtils.ExecutingDir, LOG_PATH)); ;

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

            OutputResults.PrintAndLogResult(testResults, fileLogger);


        }

       
    }
}
