using System;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Injector.App
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Drill4Net is started");

                var rep = new InjectorRepository();
                var injector = new InjectorEngine(rep);
                injector.Process(args);

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

            Console.ReadKey(true);
        }
    }
}
