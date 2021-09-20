using System;
using System.Reflection;
using System.Collections.Generic;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class ComplexData:MediumData
    {
        internal List<SimpleData> SimpleDataList { get; set; }
        internal Dictionary<int, MediumData> MediumDataDict { get; set; }
        internal MethodInfo MethodInfo { get; set; }
        internal ComplexData()
        {
            SimpleDataList = new List<SimpleData>();
            MediumDataDict = new Dictionary<int, MediumData>();
            for (var i = 0; i < CompressorConstants.DATA_COUNT; i++)
            {
                SimpleDataList.Add(new SimpleData());
            }
            for (var i = 0; i < CompressorConstants.DATA_COUNT; i++)
            {
                MediumDataDict.Add(i, new MediumData());
            }
            MethodInfo =FeedBacks.GetType().GetMethod("ToString");
        }
    }
}
