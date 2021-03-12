using System;

namespace Drill4Net.Plugins.Testing.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var profiler = new TestProfiler();
            //var requestId = "0";
            var asmName = $"Drill4Net.Target.Common.dll";
            var funcSig = "System.Void Drill4Net.Plugins.Testing.TestProfiler::Register(System.String)";
            profiler.Register($"^{asmName}^{funcSig}^If_6");
            var points = TestProfiler.GetPoints(asmName, funcSig, false);
            Console.WriteLine(string.Join(", ", points));

            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }
    }
}
