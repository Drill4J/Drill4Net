using System;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class CoverageData
    { 
        /// <summary>
        /// Dictionary of code blocks: key is point Id, value - coverage part of code by this block
        /// </summary>
        public Dictionary<int, float> BlockByPart { get; }

        public Dictionary<string, int> PointUidToEndRange { get; set; }

        /**********************************************************/

        public CoverageData()
        {
            BlockByPart = new Dictionary<int, float>();
            PointUidToEndRange = new Dictionary<string, int>();
        }

    }
}
