using System;

namespace Drill4Net.Agent.Testing.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var profiler = new TestAgent();
                //need set actual some Uid because it will be changed in the Tree data after each recompiling
                profiler.Register($"28f748b5-0fc2-41ea-a068-a5cf3b7e2e2c");
                //
                var funcs = TestAgent.GetMethods(false);
                foreach(var f in funcs.Keys)
                    Console.WriteLine($"{f}: {string.Join(", ", funcs[f])}");
                //
                Console.WriteLine("Done.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            Console.ReadKey(true);
        }
    }
}
