using System;

namespace Drill4Net.Agent.Testing.Debug
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var profiler = new TestingProfiler();
                var asmName = $"Drill4Net.Target.Common.dll";
                var funcSig = "System.Void Drill4Net.Agent.Testing.TesterProfiler::Register(System.String)";
                profiler.Register($"^{asmName}^{funcSig}^{100}^If_6");
                var points = TestingProfiler.GetPoints(asmName, funcSig, false);
                Console.WriteLine(string.Join(", ", points));

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
