﻿using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Drill4Net.Cli;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Repository;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Injector.App
{
    internal class Program
    {
        private static CliDescriptor _cliDescriptor;
        private static Logger _logger;

        /**************************************************************************/

        static async Task Main(string[] args)
        {
            AbstractRepository.PrepareEmergencyLogger();

            //program name... yep, from namespace
            var name = $"{typeof(Program).Namespace} {FileUtils.GetProductVersion(typeof(InjectorRepository))}";
            Log.Info($"{name} is starting"); //use emergency logger with simple static call until normal logger is created

            _cliDescriptor = new CliDescriptor(args, false);
            bool silent = _cliDescriptor.GetParameter(CoreConstants.ARGUMENT_SILENT) != null;
#if DEBUG
            var watcher = Stopwatch.StartNew();
#endif
            try
            {
                _logger = new TypedLogger<Program>(CoreConstants.SUBSYSTEM_INJECTOR); //real typed logger from cfg
                _logger.Debug($"Arguments: [{string.Join(" ", args)}]");

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
                        throw new ArgumentException($"Configs for injections not found: [{cfgDirArg}]");

                    // degreeParallel
                    var degreeParallelArg = _cliDescriptor.GetParameter(CoreConstants.ARGUMENT_DEGREE_PARALLELISM);
                    var degreeParallel = degreeParallelArg == null ? Environment.ProcessorCount : Convert.ToInt32(degreeParallelArg);

                    //run in parallel
                    var parOpts = new ParallelOptions { MaxDegreeOfParallelism = degreeParallel };
                    Parallel.ForEach(configs, parOpts, (cfgPath) => Process(true, cfgPath));
                }
                else //manual start
                {
                    await Process(false).ConfigureAwait(false);
                }
#if DEBUG
                #region Benchmark
                watcher.Stop();

                Console.WriteLine("");
                var duration = watcher.ElapsedMilliseconds;
                _logger.Info($"Duration of target injection: {duration} ms.");

                var benchLog = @$"{LoggerHelper.LOG_FOLDER}\benchmarkLog.txt";
                IBenchmarkLogger benchmarkFileLogger = new BenchmarkFileLogger(Path.Combine(FileUtils.ExecutingDir, benchLog));
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

        // *** The magic is here *** //
        internal static Task Process(bool fromDir, string cfgPath = null)
        {
            InjectorRepository rep = null;
            try
            {
                // the directory can contains different types of the configs
                if (fromDir && !string.IsNullOrWhiteSpace(cfgPath))
                {
                    var helper = new BaseOptionsHelper<InjectionOptions>(CoreConstants.SUBSYSTEM_INJECTOR);
                    var opts = helper.ReadOptions(cfgPath);
                    if (opts.Type != CoreConstants.SUBSYSTEM_INJECTOR)
                        return Task.CompletedTask;
                }
                rep = new InjectorRepository(cfgPath, _cliDescriptor);
                _logger.Debug(rep.Options);
                var injector = new InjectorEngine(rep);
                return injector.Process();
            }
            catch (Exception ex)
            {
                if (rep?.Options?.Silent == true)
                {
                    _logger.Warning($"{ex.Message}");
                    return Task.CompletedTask;
                }
                else
                {
                    return Task.FromException(ex);
                }
            }
        }
    }
}
