using System;
using System.Collections.Generic;
using System.Linq;

namespace Drill4Net.Profiling.Tree
{
    [Serializable]
    public class CoverageData
    { 
        /// <summary>
        /// Dictionary of code blocks: key is point Id, value - coverage part of code by this block
        /// </summary>
        public Dictionary<int, float> BlockByPart { get; }

        public Dictionary<string, int> PointUidToEndIndex { get; set; }

        /**********************************************************/

        public CoverageData()
        {
            BlockByPart = new Dictionary<int, float>();
            PointUidToEndIndex = new Dictionary<string, int>();
        }
    }
}
