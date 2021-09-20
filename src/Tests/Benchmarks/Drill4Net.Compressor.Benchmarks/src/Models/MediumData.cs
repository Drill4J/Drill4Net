using Drill4Net.Compressor.Benchmarks.Helpers;
using System;
using System.Collections.Generic;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class MediumData:SimpleData
    {
        internal double Rate3 { get; set; }
        internal decimal Price1 { get; set; }
        internal decimal Price2 { get; set; }
        internal decimal Price3 { get; set; }
        internal List<string> FeedBacks { get; set; }
        internal Guid ObjectGuid { get; set; }
        internal DateTime Date { get; set; }
        internal HashSet<string> Tags { get; set; }
        internal Dictionary<int, DateTime> Years { get; set; }

        internal MediumData()
        {
            var rnd = new Random();
            Rate3 = rnd.NextDouble() * 99 + 1;
            Price1 = 1M / 3M + rnd.Next(100, 500);
            Price2 = 79000000000000000000000000000.55M + rnd.Next(100, 500);
            Price3 = -110000000000000000000000000.55M + rnd.Next(100, 500);
            FeedBacks = new List<string>();
            Tags = new HashSet<string>();
            Years = new Dictionary<int, DateTime>();
            for (var i = 0; i < CompressorConstants.DATA_COUNT; i++)
            {
                FeedBacks.Add(PrepareData.GenerateString());
            }
            for (var i = 0; i < CompressorConstants.DATA_COUNT; i++)
            {
                Tags.Add(PrepareData.GenerateString());
            }
            for (var i = 0; i < CompressorConstants.DATA_COUNT; i++)
            {
                Years.Add(i, DateTime.Now);
            }
            Date = DateTime.Now.AddDays(rnd.Next(-100, 100));
            ObjectGuid = new Guid();
        }

    }
}
