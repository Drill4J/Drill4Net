using System;

namespace Drill4Net.Plugins.Testing.Demo
{
    class Program
    {
        static void Main(string[] args)
        {
            var profiler = new TestProfiler();
            var requestId = "4243e54508a84689b7b6575f72630260";
            var path = "Drill4Net.Target.Common.dll; System.Void Drill4Net.Target.Common.InjectTarget::IfElse_FullSimple(System.Boolean)";
            profiler.Process($"{requestId}^{path}^If_6");
            var points = TestProfiler.GetPoints(requestId, path, true);
            Console.WriteLine(string.Join(", ", points));

            Console.WriteLine("Done.");
            Console.ReadKey(true);
        }
    }
}
