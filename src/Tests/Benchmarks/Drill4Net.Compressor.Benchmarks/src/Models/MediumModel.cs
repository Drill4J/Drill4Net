using Drill4Net.Compressor.Benchmarks.Helpers;
using System;
using System.Collections.Generic;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class MediumModel:SimpleModel
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
        internal TimeSpan[] TimeSpans { get; set; }

        internal MediumModel()
        {
            Rate3 = CompressorConfig.rnd.NextDouble() * 99 + 1;
            Price1 = 1M / 3M + CompressorConfig.rnd.Next(100, 500);
            Price2 = 79000000000000000000000000000.55M + CompressorConfig.rnd.Next(100, 500);
            Price3 = -110000000000000000000000000.55M + CompressorConfig.rnd.Next(100, 500);
            Date = DateTime.Now.AddDays(CompressorConfig.rnd.Next(-100, 100));
            ObjectGuid = new Guid();
            FeedBacks = new List<string>();
            Tags = new HashSet<string>();
            Years = new Dictionary<int, DateTime>();
            TimeSpans = new TimeSpan[CompressorConfig.DATA_COUNT];
            for (var i = 0; i < CompressorConfig.DATA_COUNT; i++)
            {
                TimeSpans[i]=DateTime.Now- Date;
            }
            for (var i = 0; i < CompressorConfig.DATA_COUNT; i++)
            {
                FeedBacks.Add(PrepareData.GenerateString());
            }
            for (var i = 0; i < CompressorConfig.DATA_COUNT; i++)
            {
                Tags.Add(PrepareData.GenerateString());
            }
            for (var i = 0; i < CompressorConfig.DATA_COUNT; i++)
            {
                Years.Add(i, DateTime.Now);
            }

        }

    }
}
