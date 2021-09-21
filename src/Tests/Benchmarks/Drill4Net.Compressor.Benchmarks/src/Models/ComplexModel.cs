using System;
using System.Reflection;
using System.Collections.Generic;

namespace Drill4Net.Compressor.Benchmarks.Models
{
    [Serializable]
    internal class ComplexModel: MediumModel
    {
        internal List<SimpleModel> SimpleModelList { get; set; }
        internal Dictionary<int, MediumModel> MediumModelDict { get; set; }
        internal MethodInfo MethodInfo { get; set; }
        internal ComplexModel()
        {
            SimpleModelList = new List<SimpleModel>();
            MediumModelDict = new Dictionary<int, MediumModel>();
            for (var i = 0; i < CompressorConfig.DATA_COUNT; i++)
            {
                SimpleModelList.Add(new SimpleModel());
            }
            for (var i = 0; i < CompressorConfig.DATA_COUNT; i++)
            {
                MediumModelDict.Add(i, new MediumModel());
            }
            MethodInfo =FeedBacks.GetType().GetMethod("ToString");
        }
    }
}
