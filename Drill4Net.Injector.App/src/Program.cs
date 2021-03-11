using System;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Injector.App
{
    class Program
    {
        static void Main(string[] args)
        {
            IInjectorRepository rep = null;
            try
            {
                //name of program... from namespace
                var name = typeof(Program).Namespace.Split('.')[0];
                Console.WriteLine($"{name} is started");

                rep = new InjectorRepository(args);
                var injector = new InjectorEngine(rep);
                injector.Process();

                // for Testing project
                //var testsOpts = opts.Tests;
                //if (module.Name == testsOpts.AssemblyName)
                //{
                //    var testingPrjDir = testsOpts.Directory;
                //    if (!Directory.Exists(testingPrjDir))
                //        Directory.CreateDirectory(testingPrjDir);
                //    var testPath = Path.Combine(testingPrjDir, testsOpts.AssemblyName);
                //    File.Copy(modifiedPath, testPath, true);
                //}
                
                 Console.WriteLine("Injection is done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (rep?.Options?.Silent == false)
                Console.ReadKey(true);
        }
    }
}
