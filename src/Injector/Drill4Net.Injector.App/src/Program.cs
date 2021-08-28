using System;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using Drill4Net.Core.Repository;
using Drill4Net.BanderLog;

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

                Log.Debug($"Arguments: {args}");
                Log.Debug($"Options: {rep.Options}");

                var injector = new InjectorEngine(rep);

                injector.Process();

                Log.Info("Injection is done.");
                Log.Trace("");
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
            }

            Log.Shutdown();
            if (rep?.Options?.Silent == false)
                Console.ReadKey(true);
        }
    }
}
