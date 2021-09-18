using System;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class SimpleData
    {
        internal int Year { get; set; }
        internal int NumberOfPages {get; set;}
        internal double Rate1 { get; set; }
        internal double Rate2 { get; set; }
        internal double Rate3 { get; set; }
        internal string Title { get; set; }
        internal string Notes { get; set; }
    }
}
