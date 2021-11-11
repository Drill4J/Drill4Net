using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Drill4Net.Common;
using Drill4Net.BanderLog;
using Drill4Net.Injector.Core;
using Drill4Net.Core.Repository;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Injector.App
{
    class Program
    {
        private static Logger _logger;
        private const string LOG_PATH = @"logs\benchmarkLog.txt";

        /**************************************************************************/

        static async Task Main(string[] args)
        {
            AbstractRepository.PrepareEmergencyLogger();
            //program name... yep, from namespace
            var name = typeof(Program).Namespace + " " + FileUtils.GetProductVersion(typeof(InjectorRepository));
            Log.Info($"{name} is starting"); //using emergency logger by simple static call

            IInjectorRepository rep = null;
            try
            {
                rep = new InjectorRepository(args);
                _logger = new TypedLogger<Program>(rep.Subsystem); //real typed logger from cfg

                //_logger.Debug("Arguments: {@Args}", args);
                //_logger.Debug("Options: {@Options}", rep.Options);
                _logger.Debug(args);
                _logger.Debug(rep.Options);

                var injector = new InjectorEngine(rep);
#if DEBUG
                var watcher = Stopwatch.StartNew();
#endif
                await injector.Process()
                    .ConfigureAwait(false);
#if DEBUG
                watcher.Stop();

                Console.WriteLine("");
                var duration = watcher.ElapsedMilliseconds;
                _logger.Info($"Duration of target injection: {duration} ms.");

                IBenchmarkLogger benchmarkFileLogger = new BenchmarkFileLogger(Path.Combine(FileUtils.ExecutingDir, LOG_PATH));
                BenchmarkLog.WriteBenchmarkToLog(benchmarkFileLogger, AssemblyGitInfo.GetSourceBranchName(), AssemblyGitInfo.GetCommit(),
                    duration.ToString());

                _logger.Info("Done.");
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
            if (rep?.Options?.Silent == false)
                Console.ReadKey(true);
        }
    }
}
