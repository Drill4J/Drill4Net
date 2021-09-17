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
        private static Random rnd = new Random();
        const int count = 100;
        /****************************************************/

        /// <summary>
        ///Generate Simple Data
        /// <summary>
        /// <return></return>
        internal static SimpleData GenerateSimpleData ()
        {
            var data = new SimpleData();
            data.Year = rnd.Next(1900, 2000);
            data.NumberOfPages= rnd.Next(100, 1000);
            data.Rate1= rnd.NextDouble()*99 +1;
            data.Rate2 = rnd.NextDouble() * 99 + 1;
            data.Rate3 = rnd.NextDouble() * 99 + 1;
            data.Title = GenerateString();
            data.Notes = GenerateString();

            return data;
        }

        /// <summary>
        ///Generate Medium Data
        /// <summary>
        /// <return></return>
        internal static MediumData GenerateMediumData()
        {
            var data = new MediumData();
            data.Year = rnd.Next(1900, 2000);
            data.NumberOfPages = rnd.Next(100, 1000);
            data.Rate1 = rnd.NextDouble() * 99 + 1;
            data.Rate2 = rnd.NextDouble() * 99 + 1;
            data.Rate3 = rnd.NextDouble() * 99 + 1;
            data.Title = GenerateString();
            data.Notes = GenerateString();
            data.Price1=100.55M + rnd.Next(100, 500);
            data.Price2 = 100.55M + rnd.Next(100, 500);
            data.Price3 = 100.55M + rnd.Next(100, 500);
            data.FeedBacks = new List<string>();
            data.Tags = new HashSet<string>();
            for (var i=0; i<count; i++)
            {
                data.FeedBacks.Add(GenerateString());
            }
            for (var i = 0; i < count; i++)
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
        internal static ComplexData GenerateComplexData()
        {
            var data = new ComplexData();
            data.Year = rnd.Next(1900, 2000);
            data.NumberOfPages = rnd.Next(100, 1000);
            data.Rate1 = rnd.NextDouble() * 99 + 1;
            data.Rate2 = rnd.NextDouble() * 99 + 1;
            data.Rate3 = rnd.NextDouble() * 99 + 1;
            data.Title = GenerateString();
            data.Notes = GenerateString();
            data.Price1 = 100.55M + rnd.Next(100, 500);
            data.Price2 = 100.55M + rnd.Next(100, 500);
            data.Price3 = 100.55M + rnd.Next(100, 500);
            data.FeedBacks = new List<string>();
            data.Tags = new HashSet<string>();
            for (var i = 0; i < count; i++)
            {
                data.FeedBacks.Add(GenerateString());
            }
            for (var i = 0; i < count; i++)
            {
                data.Tags.Add(GenerateString());
            }
            data.Date = DateTime.Now.AddDays(rnd.Next(-100, 100));
            data.ObjectGuid = new Guid();
            data.SimpleDataList = new List<SimpleData>();
            data.MediumDataDict = new Dictionary<int, MediumData>();
            for (var i = 0; i < count; i++)
            {
                data.SimpleDataList.Add(new SimpleData());
            }
            for (var i = 0; i < count; i++)
            {
                data.MediumDataDict.Add(i, new MediumData());
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
            IInjectorRepository rep = null;
            rep = new InjectorRepository(cfgPath);
            var injector = new InjectorEngine(rep);
            var tree = await injector.Process();
            return tree;
        }

        private static string GenerateString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var t= new string(Enumerable.Range(1, rnd.Next(10, 20)).Select(c => chars[rnd.Next(chars.Length)]).ToArray());
            return new string(Enumerable.Range(1, rnd.Next(10, 20)).Select(c => chars[rnd.Next(chars.Length)]).ToArray());

        }
    }
}
