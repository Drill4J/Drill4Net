using System;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using Serilog;

namespace Drill4Net.Injector.App
{
    class Program
    {
        static void Main(string[] args)
        {
            IInjectorRepository rep = null;
            try
            {
                //program name... from namespace
                var name = typeof(Program).Namespace.Split('.')[0];
                Console.WriteLine($"{name} is started");

                rep = new InjectorRepository(args);
                rep.PrepareLogger();
                Log.Debug("Options: {@Options}", rep.Options);

                var injector = new InjectorEngine(rep);
                injector.Process();

                Log.Information("Injection is done.");
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
