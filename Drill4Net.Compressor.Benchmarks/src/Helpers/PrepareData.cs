using System;
using System.IO;
using System.Linq;
using Drill4Net.Common;
using Drill4Net.Profiling.Tree;
using Drill4Net.Compressor.Benchmarks.Dto;


namespace Drill4Net.Compressor.Benchmarks.src.Helpers
{
    internal static class PrepareData
    {
        private static Random rnd = new Random();
        const int count = 100;
        /****************************************************/

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
            for (var i = 0; i < count; i++)
            {
                data.SimpleDataList.Add(new SimpleData());
            }
            for (var i = 0; i < count; i++)
            {
                data.MediumDataDict.Add(i,new MediumData());
            }
            data.FileInfo = new FileInfo(GenerateString());
            data.MethodInfo = data.FileInfo.GetType().GetMethod("ToString");
            data.DirectoryInfo = new DirectoryInfo(FileUtils.ExecutingDir);

            return data;
        }
        internal static InjectedSolution GenerateInjectedSolution(string cfgPath)
        {

        }
        private static string GenerateString()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(Enumerable.Range(1, rnd.Next(10, 20)).Select(c => chars[rnd.Next(chars.Length)]).ToArray());

        }
    }
}
