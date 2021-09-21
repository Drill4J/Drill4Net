﻿using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Engine;

namespace Drill4Net.Compressor.Benchmarks.Helpers
{
    internal static class PrepareData
    {
        /// <summary>
        ///Generate InjectedSolution tree
        /// <summary>
        /// <return></return>
        internal static async Task<InjectedSolution> GenerateInjectedSolutionAsync(string cfgName)
        {
            var cfgPath = Path.Combine(FileUtils.ExecutingDir, cfgName);
            IInjectorRepository rep = null;
            rep = new InjectorRepository(cfgPath);
            var injector = new InjectorEngine(rep);
            var tree = await injector.Process();
            return tree;
        }

        internal static string GenerateString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Range(1, CompressorConfig.rnd.Next(10, 20)).Select(_ => chars[CompressorConfig.rnd.Next(chars.Length)]).ToArray());
        }
    }
}
