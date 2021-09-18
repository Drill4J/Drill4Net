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
        private static readonly Random rnd = new Random(DateTime.Now.Millisecond);
        private const int _count = 100;

        /*****************************************************************************/

        /// <summary>
        ///Generate Simple Data
        /// <summary>
        /// <return></return>
        internal static SimpleModel GenerateSimpleData()
        {
            return new SimpleModel
            {
                Year = rnd.Next(1900, 2000),
                NumberOfPages = rnd.Next(100, 1000),
                Rate1 = rnd.NextDouble() * 99 + 1,
                Rate2 = rnd.NextDouble() * 99 + 1,
                Rate3 = rnd.NextDouble() * 99 + 1,
                Title = GenerateString(),
                Notes = GenerateString()
            };
        }

        /// <summary>
        ///Generate Medium Data
        /// <summary>
        /// <return></return>
        internal static MediumModel GenerateMediumData()
        {
            var data = new MediumModel
            {
                Year = rnd.Next(1900, 2000),
                NumberOfPages = rnd.Next(100, 1000),
                Rate1 = rnd.NextDouble() * 99 + 1,
                Rate2 = rnd.NextDouble() * 99 + 1,
                Rate3 = rnd.NextDouble() * 99 + 1,
                Title = GenerateString(),
                Notes = GenerateString(),
                Price1 = 100.55M + rnd.Next(100, 500),
                Price2 = 100.55M + rnd.Next(100, 500),
                Price3 = 100.55M + rnd.Next(100, 500),
                FeedBacks = new List<string>(),
                Tags = new HashSet<string>()
            };

            for (var i = 0; i < _count; i++)
            {
                data.FeedBacks.Add(GenerateString());
            }

            for (var i = 0; i < _count; i++)
            {
                data.Tags.Add(GenerateString());
            }

            data.Date = DateTime.Now.AddDays(rnd.Next(-100, 100));
            data.ObjectGuid = new Guid();

            return data;
        }

        /// <summary>
        ///Generate Complex Data
        /// <summary>
        /// <return></return>
        internal static ComplexModel GenerateComplexData()
        {
            var data = new ComplexModel
            {
                Year = rnd.Next(1900, 2000),
                NumberOfPages = rnd.Next(100, 1000),
                Rate1 = rnd.NextDouble() * 99 + 1,
                Rate2 = rnd.NextDouble() * 99 + 1,
                Rate3 = rnd.NextDouble() * 99 + 1,
                Title = GenerateString(),
                Notes = GenerateString(),
                Price1 = 100.55M + rnd.Next(100, 500),
                Price2 = 100.55M + rnd.Next(100, 500),
                Price3 = 100.55M + rnd.Next(100, 500),
                FeedBacks = new List<string>(),
                Tags = new HashSet<string>()
            };

            for (var i = 0; i < _count; i++)
            {
                data.FeedBacks.Add(GenerateString());
            }

            for (var i = 0; i < _count; i++)
            {
                data.Tags.Add(GenerateString());
            }

            data.Date = DateTime.Now.AddDays(rnd.Next(-100, 100));
            data.ObjectGuid = new Guid();
            data.SimpleDataList = new List<SimpleModel>();
            data.MediumDataDict = new Dictionary<int, MediumModel>();

            for (var i = 0; i < _count; i++)
            {
                data.SimpleDataList.Add(new SimpleModel());
            }

            for (var i = 0; i < _count; i++)
            {
                data.MediumDataDict.Add(i, new MediumModel());
            }

            //data.FileInfo = new FileInfo(GenerateString());
            data.MethodInfo = data.FeedBacks.GetType().GetMethod("ToString");
            //data.DirectoryInfo = new DirectoryInfo(FileUtils.ExecutingDir);

            return data;
        }

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

        private static string GenerateString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var t = new string(Enumerable.Range(1, rnd.Next(10, 20)).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
            return new string(Enumerable.Range(1, rnd.Next(10, 20)).Select(_ => chars[rnd.Next(chars.Length)]).ToArray());
        }
    }
}
