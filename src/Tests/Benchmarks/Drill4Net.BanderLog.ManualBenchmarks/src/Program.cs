using System;
using System.Threading.Tasks;

namespace Drill4Net.BanderLog.ManualBenchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            using var tests = new Tests();
            Console.WriteLine("Simple tests:\n");

            tests.RunSimpleTests(2500);
            Console.WriteLine();
            tests.RunSimpleTests(10000);
            Console.WriteLine();
            tests.RunSimpleTests(100000);
            Console.WriteLine();

            Console.WriteLine("Multi-task tests:\n");
            tests.RunMultiTaskTests(10000,2);
            Console.WriteLine();
            tests.RunMultiTaskTests(10000, 5);
            Console.WriteLine();
            tests.RunMultiTaskTests(10000, 10);
        }
    }
}
