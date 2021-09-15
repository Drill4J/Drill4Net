using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Compressor.Benchmarks.Dto
{
    internal class MediumData:SimpleData
    {
        internal decimal Price1 { get; set; }
        internal decimal Price2 { get; set; }
        internal decimal Price3 { get; set; }
        internal List<string> FeedBacks { get; set; }
        internal Guid ObjectGuid { get; set; }
        internal DateTime Date { get; set; }
        internal HashSet<string> Tags { get; set; }

    }
}
