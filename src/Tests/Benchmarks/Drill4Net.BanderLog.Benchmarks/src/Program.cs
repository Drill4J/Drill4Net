using BenchmarkDotNet.Running;

namespace Drill4Net.BanderLog.Benchmarks
{
    class Program
    {
        static void Main (string[] args)
        {
            BenchmarkRunner.Run<Tests>();
        }
    }
}
