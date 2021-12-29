using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Injector.App
{
    internal class Program
    {
        private static CliDescriptor _cliDescriptor;
        private static Logger _logger;
        private const string LOG_PATH = @"logs\benchmarkLog.txt";

        /**************************************************************************/

        static async Task Main(string[] args)
        {
            AbstractRepository.PrepareEmergencyLogger();

            //program name... yep, from namespace
            var name = $"{typeof(Program).Namespace} {FileUtils.GetProductVersion(typeof(InjectorRepository))}";
            Log.Info($"{name} is starting"); //use emergency logger with simple static call until normal logger is created

            bool silent = false;
#if DEBUG
            var watcher = Stopwatch.StartNew();
#endif
            try
            {
                _logger = new TypedLogger<Program>(CoreConstants.SUBSYSTEM_INJECTOR); //real typed logger from cfg
                _logger.Debug(args);
                _cliDescriptor = new CliDescriptor(args, false);

                //silent
                var silentArg = _cliDescriptor.GetParameter(CoreConstants.ARGUMENT_SILENT);
                silent = silentArg != null;

                // degreeParallel
                var degreeParallelArg = _cliDescriptor.GetParameter(CoreConstants.ARGUMENT_DEGREE_PARALLELISM);
                var degreeParallel = degreeParallelArg == null ? Environment.ProcessorCount : Convert.ToInt32(degreeParallelArg);

                //loop count
                var loops = 1;
                var cfgDirArg = _cliDescriptor.GetParameter(CoreConstants.ARGUMENT_CONFIG_DIR);
                if (cfgDirArg != null) //start by arguments in CLI for multiple target's configs
                {
                    if (!Directory.Exists(cfgDirArg))
                        throw new ArgumentException($"Directory from CLI arguments does not exist: [{cfgDirArg}]");
                    var configs = Directory.GetFiles(cfgDirArg, "*.yml");
                    loops = configs.Length;
                    if (loops == 0)
                        throw new ArgumentException($"Configs not found for injections in: [{cfgDirArg}]");

                    //run in parallel
                    var parOpts = new ParallelOptions { MaxDegreeOfParallelism = degreeParallel };
                    Parallel.ForEach(configs, parOpts, (cfgPath) => Process(cfgPath));
                }
                else //manual start
                {
                    await Process().ConfigureAwait(false);
                }
#if DEBUG
                #region Benchmark
                watcher.Stop();

                Console.WriteLine("");
                var duration = watcher.ElapsedMilliseconds;
                _logger.Info($"Duration of target injection: {duration} ms.");

                IBenchmarkLogger benchmarkFileLogger = new BenchmarkFileLogger(Path.Combine(FileUtils.ExecutingDir, LOG_PATH));
                BenchmarkLog.WriteBenchmarkToLog(benchmarkFileLogger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(),
                    duration.ToString());

                _logger.Info("Done.");
                #endregion
#endif
            }
            catch (Exception ex)
            {
                if (_logger == null)
                    Log.Fatal(ex);
                else
                    _logger.Fatal(ex);
            }

            Log.Shutdown();
            if (!silent)
                Console.ReadKey(true);
        }

        internal static Task Process(string cfgPath = null)
        {
            var rep = cfgPath == null ? new InjectorRepository(_cliDescriptor) : new InjectorRepository(cfgPath);
            _logger.Debug(rep.Options);

            var injector = new InjectorEngine(rep);
            return injector.Process(); //the magic is here
        }
    }
}
