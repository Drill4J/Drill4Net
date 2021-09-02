using System;
using System.Diagnostics;
using Drill4Net.BanderLog;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using Drill4Net.Core.Repository;
using Drill4Net.Injector.App.Helpers;
using System.IO;

namespace Drill4Net.Injector.App
{
    class Program
    {
        private static Logger _logger;

        /**************************************************************************/

        static void Main(string[] args)
        {
            AbstractRepository.PrepareEmergencyLogger();
            //program name... yep, from namespace
            var name = typeof(Program).Namespace.Split('.')[0];
            Log.Info($"{name} is starting"); //using emergency logger by simple static call

            IInjectorRepository rep = null;
            try
            {
                rep = new InjectorRepository(args);
                _logger = new TypedLogger<Program>(rep.Subsystem); //real typed logger from cfg

                //_logger.Debug("Arguments: {@Args}", args);
                // _logger.Debug("Options: {@Options}", rep.Options);
                _logger.Debug(args);
                _logger.Debug(rep.Options);

                var injector = new InjectorEngine(rep);
#if DEBUG
                var watcher = Stopwatch.StartNew();
#endif
                injector.Process();
#if DEBUG
                watcher.Stop();
#endif
                _logger.Info("Injection is done.");
                Console.WriteLine("");
#if DEBUG
                var msg = $"Duration of target injection: {watcher.ElapsedMilliseconds} ms.";
                var benchmarkFileLogger = new BenchmarkFileLogger(Path.GetFullPath(@"logs\benchmarkLog.txt"));
                _logger.Info(msg);
                BenchmarkLog.WriteBenchmarkToLog(benchmarkFileLogger, injector.GetSourceBranchName(), injector.GetCommit(), msg);
#endif
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex);
            }

            Log.Shutdown();
            if (rep?.Options?.Silent == false)
                Console.ReadKey(true);
        }
    }
}
