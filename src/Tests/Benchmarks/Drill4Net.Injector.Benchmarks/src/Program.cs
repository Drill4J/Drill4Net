using BenchmarkDotNet.Running;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using System;
using System.IO;

namespace Drill4Net.Injector.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Tests>();
        }
    }
}
