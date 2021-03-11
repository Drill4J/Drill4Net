using System;

namespace Drill4Net.Plugins.Testing.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var profiler = new TestProfiler();
            var requestId = "0";
            var asmName = $"Drill4Net.Target.Common.dll";
            var funcSig = "System.Void Drill4Net.Target.Common.InjectTarget::IfElse_FullSimple(System.Boolean)";
            profiler.Register($"{requestId}^{asmName}^{funcSig}^If_6");
            var points = TestProfiler.GetPoints(requestId, asmName, funcSig, true);
            Console.WriteLine(string.Join(", ", points));

            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }
    }
}
