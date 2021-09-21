using System.IO;
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
            IInjectorRepository rep = new InjectorRepository(cfgPath);
            var injector = new InjectorEngine(rep);
            var tree = await injector.Process().ConfigureAwait(false);
            return tree;
        }

        internal static string GenerateString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(
                Enumerable.Range(1, CompressorConfigurator.Rnd.Next(10, 20))
                .Select(_ => chars[CompressorConfigurator.Rnd.Next(chars.Length)])
                .ToArray());
        }
    }
}
