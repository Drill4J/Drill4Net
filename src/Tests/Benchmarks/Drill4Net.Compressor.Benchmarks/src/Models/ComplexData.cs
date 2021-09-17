using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class ComplexData:MediumData
    {
        internal List<SimpleData> SimpleDataList { get; set; }
        internal Dictionary<int, MediumData> MediumDataDict { get; set; }
        //internal FileInfo FileInfo { get; set; }
        //internal DirectoryInfo DirectoryInfo { get; set; }
        internal MethodInfo MethodInfo { get; set; }
    }
}
