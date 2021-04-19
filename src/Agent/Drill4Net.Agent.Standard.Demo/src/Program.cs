using System;

namespace Drill4Net.Agent.Standard.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var asmName = $"Drill4Net.Target.Common.dll";
                const string funcSig = "System.Void Drill4Net.Agent.Standard.StandardProfiler::Register(System.String)";
                StandardProfiler.RegisterStatic($"^{asmName}^{funcSig}^100^If_6");
                //
                var funcs = StandardProfiler.GetFunctions(false);
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