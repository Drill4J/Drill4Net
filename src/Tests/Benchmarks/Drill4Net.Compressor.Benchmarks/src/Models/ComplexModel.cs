using System;
using System.Reflection;
using System.Collections.Generic;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class ComplexModel: MediumModel
    {
        internal List<SimpleModel> SimpleDataList { get; set; }
        internal Dictionary<int, MediumModel> MediumDataDict { get; set; }
        //internal FileInfo FileInfo { get; set; }
        //internal DirectoryInfo DirectoryInfo { get; set; }
        internal MethodInfo MethodInfo { get; set; }
    }
}
