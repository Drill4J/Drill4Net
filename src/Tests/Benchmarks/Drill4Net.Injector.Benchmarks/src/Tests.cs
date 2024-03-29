﻿using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Injector.Core;
using Drill4Net.Injector.Engine;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using System.IO;
using BenchmarkDotNet.Diagnosers;

namespace Drill4Net.Injector.Benchmarks
{
    /******************************************************************************************
     To perform benchmark test run Drill4Net.Injector.Benchmarks with Release|Any CPU
     In listing you can find path to EventPipeProfiler json generated file that can be
     converted to plot which can be analyzed using SpeedScope (), PerfView, and Visual Studio Profiler
    ******************************************************************************************/

    /// <summary>
    /// Benchmarks for Injector
    /// </summary>
    [MemoryDiagnoser]
    [SimpleJob(RunStrategy.ColdStart)]
    [HardwareCounters(HardwareCounter.Timer, HardwareCounter.BranchMispredictions, HardwareCounter.BranchInstructions)]
    [EventPipeProfiler(EventPipeProfile.CpuSampling)]
    [HtmlExporter]
    [Config(typeof(Config))]
    public class Tests
    {         
        readonly string rootFolder = Environment.CurrentDirectory;
        InjectorEngine injector = null;

        [Params("inj_Std.yml")]
        public string CfgName { get; set; }

        /******************************************************************************************/

        private class Config : ManualConfig
        {
            public Config()
            {
                WithOptions(ConfigOptions.DisableOptimizationsValidator);
            }
        }

        [GlobalSetup]
        public void GlobalSetup()
        {
            var cfgPath = Path.Combine(rootFolder, CfgName);
            IInjectorRepository rep = null;
            rep = new InjectorRepository(cfgPath);
            injector= new InjectorEngine(rep);
        }

        /******************************************************************************************/

        [Benchmark]
        public void injectorProcess()
        {
            injector.Process();
        }
    }
}
