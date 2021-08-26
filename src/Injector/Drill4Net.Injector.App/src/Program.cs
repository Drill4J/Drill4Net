using System;
using System.Diagnostics;
using Serilog;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Injector.App
{
    class Program
    {
        static void Main(string[] args)
        {
            //program name... yep, from namespace
            var name = typeof(Program).Namespace.Split('.')[0];
            Console.WriteLine($"{name} is starting");

            AbstractRepository<InjectorOptions>.PrepareInitLogger();
            IInjectorRepository rep = null;
            try
            {
                rep = new InjectorRepository(args);

                Log.Debug("Arguments: {@Args}", args);
                Log.Debug("Options: {@Options}", rep.Options);

                var injector = new InjectorEngine(rep);
#if DEBUG
                var watcher = Stopwatch.StartNew();
#endif
                injector.Process();
#if DEBUG
                watcher.Stop();
#endif

                Log.Information("Injection is done.");
                Log.Verbose("");
#if DEBUG
                Log.Information($"Duration of target injection: {watcher.ElapsedMilliseconds} ms.");
#endif
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            Log.CloseAndFlush();
            if (rep?.Options?.Silent == false)
                Console.ReadKey(true);
        }
    }
}
