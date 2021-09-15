using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Drill4Net.Compressor.Benchmarks.Dto
{
    internal class ComplexData:MediumData
    {
        internal List<SimpleData> SimpleDataList { get; set; }
        internal Dictionary<int, MediumData> MediumDataDict { get; set; }

        internal FileInfo FileInfo { get; set; }
        internal DirectoryInfo DirectoryInfo { get; set; }
        internal MethodInfo MethodInfo { get; set; }
    }
}
