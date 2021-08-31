using System;
using System.Diagnostics;
using Drill4Net.BanderLog;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using Drill4Net.Core.Repository;

namespace Drill4Net.Injector.App
{
    class Program
    {
        private static Logger _logger;

        /**************************************************************************/

        static void Main(string[] args)
        {
            //program name... yep, from namespace
            var name = typeof(Program).Namespace.Split('.')[0];
            Console.WriteLine($"{name} is starting");

            AbstractRepository.PrepareEmergencyLogger();
            IInjectorRepository rep = null;
            try
            {
                rep = new InjectorRepository(args);
                _logger = new TypedLogger<Program>(rep.Subsystem);

                _logger.Debug($"Arguments: {args}");
                _logger.Debug($"Options: {rep.Options}");

                var injector = new InjectorEngine(rep);
#if DEBUG
                var watcher = Stopwatch.StartNew();
#endif
                injector.Process();
#if DEBUG
                watcher.Stop();
#endif
                _logger.Info("Injection is done.");
                _logger.Trace("");
#if DEBUG
                _logger.Info($"Duration of target injection: {watcher.ElapsedMilliseconds} ms.");
#endif
            }
            catch (Exception ex)
            {
                _logger.Error(ex.ToString());
            }

            Log.Shutdown();
            if (rep?.Options?.Silent == false)
                Console.ReadKey(true);
        }
    }
}
