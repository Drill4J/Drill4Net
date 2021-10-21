using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Collections.Generic;
using K4os.Compression.LZ4;
using Microsoft.Extensions.Logging;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Profiling.Tree;
using Drill4Net.Compressor.Benchmarks.Models;
using Drill4Net.Compressor.Benchmarks.Helpers;

namespace Drill4Net.Compressor.Benchmarks
{
    /// <summary>
    ///Run Tests
    /// <summary>
    class Program
    {

        static async Task Main(string[] args)
        {
            IBenchmarkLogger fileLogger = new BenchmarkFileLogger(Path.Combine(FileUtils.ExecutingDir, CompressorConfigurator.BENCHMARK_LOG_PATH));
            var logBld = new LogBuilder();
            var logger = logBld.CreateStandardLogger(Path.Combine(FileUtils.GetExecutionDir(), CompressorConfigurator.LOG_PATH));
            var tests = new Tests(logger);
            var cfgPath = Path.Combine(FileUtils.ExecutingDir, CompressorConfigurator.CFG_NAME);

            logger.LogInformation("Prepare test data");
            var simpleData = new SimpleModel();
            var simpleDataBytes = Serializer.ToArray(simpleData);
            var mediumData = new MediumModel();
            var mediumDataBytes = Serializer.ToArray(mediumData);
            var complexData = new ComplexModel();
            var complexDataBytes = Serializer.ToArray<ComplexModel>(complexData);
            var injectedData = await PrepareData.GenerateInjectedSolutionAsync(cfgPath);
            var injectedDataBytes = Serializer.ToArray<InjectedSolution>(injectedData);
            var testResults = new List<TestResult>();

            logger.LogInformation( "Start tests");
            try
            {
                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS, simpleDataBytes, CompressionLevel.Optimal, ModelTypes.Simple));
                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS, simpleDataBytes, CompressionLevel.Fastest, ModelTypes.Simple));

                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS, mediumDataBytes, CompressionLevel.Optimal, ModelTypes.Medium));
                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS, mediumDataBytes, CompressionLevel.Fastest, ModelTypes.Medium));

                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS, complexDataBytes, CompressionLevel.Optimal, ModelTypes.Complex));
                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS, complexDataBytes, CompressionLevel.Fastest, ModelTypes.Complex));

                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS_INJ, injectedDataBytes, CompressionLevel.Optimal, ModelTypes.InjectedSolution));
                testResults.Add(tests.DeflateTest(CompressorConfigurator.ITERATIONS_INJ, injectedDataBytes, CompressionLevel.Fastest, ModelTypes.InjectedSolution));

                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, simpleDataBytes, LZ4Level.L03_HC, ModelTypes.Simple));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, simpleDataBytes, LZ4Level.L10_OPT, ModelTypes.Simple));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, simpleDataBytes, LZ4Level.L00_FAST, ModelTypes.Simple));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, simpleDataBytes, LZ4Level.L09_HC, ModelTypes.Simple));

                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, mediumDataBytes, LZ4Level.L03_HC, ModelTypes.Medium));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, mediumDataBytes, LZ4Level.L10_OPT, ModelTypes.Medium));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, mediumDataBytes, LZ4Level.L00_FAST, ModelTypes.Medium));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, complexDataBytes, LZ4Level.L09_HC, ModelTypes.Medium));

                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, complexDataBytes, LZ4Level.L03_HC, ModelTypes.Complex));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, complexDataBytes, LZ4Level.L10_OPT, ModelTypes.Complex));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, complexDataBytes, LZ4Level.L00_FAST, ModelTypes.Complex));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS, complexDataBytes, LZ4Level.L09_HC, ModelTypes.Complex));

                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS_INJ, injectedDataBytes, LZ4Level.L03_HC, ModelTypes.InjectedSolution));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS_INJ, injectedDataBytes, LZ4Level.L10_OPT, ModelTypes.InjectedSolution));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS_INJ, injectedDataBytes, LZ4Level.L00_FAST, ModelTypes.InjectedSolution));
                testResults.Add(tests.LZ4Test(CompressorConfigurator.ITERATIONS_INJ, injectedDataBytes, LZ4Level.L09_HC, ModelTypes.InjectedSolution));

                ResultReporter.PrintAndLogResult(testResults, fileLogger);
            }
            catch (Exception ex)
            {
                logger.LogError(CommonUtils.GetExceptionDescription(ex));
            }
            finally
            {
                logger.Shutdown();
            }
        }
    }
}
