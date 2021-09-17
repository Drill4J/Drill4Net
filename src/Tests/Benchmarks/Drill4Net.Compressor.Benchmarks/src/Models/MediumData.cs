using System;
using System.Collections.Generic;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class MediumData
    {
        internal int Year { get; set; }
        internal int NumberOfPages { get; set; }
        internal double Rate1 { get; set; }
        internal double Rate2 { get; set; }
        internal double Rate3 { get; set; }
        internal string Title { get; set; }
        internal string Notes { get; set; }
        internal decimal Price1 { get; set; }
        internal decimal Price2 { get; set; }
        internal decimal Price3 { get; set; }
        internal List<string> FeedBacks { get; set; }
        internal Guid ObjectGuid { get; set; }
        internal DateTime Date { get; set; }
        internal HashSet<string> Tags { get; set; }
    }
}
