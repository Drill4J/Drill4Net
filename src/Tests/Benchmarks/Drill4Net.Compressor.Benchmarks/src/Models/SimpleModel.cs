using System;
using Drill4Net.Compressor.Benchmarks.Helpers;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class SimpleModel
    {
        internal int Year { get; set; }
        internal int NumberOfPages {get; set;}
        internal double Rate1 { get; set; }
        internal double Rate2 { get; set; }
        internal string Title { get; set; }
        internal string Notes { get; set; }
        
        internal SimpleData()
        {
            var rnd = new Random();
            Year = rnd.Next(1900, 2000);
            NumberOfPages = rnd.Next(100, 1000);
            Rate1 = rnd.NextDouble() * 99 + 1;
            Rate2 = rnd.NextDouble() * 99 + 1;         
            Title = PrepareData.GenerateString();
            Notes = PrepareData.GenerateString();
        }
    }
}
