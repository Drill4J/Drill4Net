using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Drill4Net.Common;
using Drill4Net.Injector.Core;
using Drill4Net.Profiling.Tree;
using Drill4Net.Injector.Engine;
using Drill4Net.Compressor.Benchmarks.Models;

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
            var t= new string(Enumerable.Range(1, CompressorConfig.rnd.Next(10, 20)).
                Select(c => chars[CompressorConfig.rnd.Next(chars.Length)]).ToArray());
            return new string(Enumerable.Range(1, CompressorConfig.rnd.Next(10, 20)).
                Select(c => chars[CompressorConfig.rnd.Next(chars.Length)]).ToArray());
        }
    }
}
