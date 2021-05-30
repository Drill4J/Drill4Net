﻿using System;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// Data of the method's coverage
    /// </summary>
    [Serializable]
    public class CoverageData
    { 
        /// <summary>
        /// Dictionary of code blocks: key is point Id, value - coverage part of code by this block
        /// </summary>
        public Dictionary<int, float> BlockByPart { get; }

        /// <summary>
        /// Binding the cross-point by Uid to last index of the block's range in the probe list of method
        /// </summary>
        public Dictionary<string, int> PointToBlockEnds { get; set; }

        /**************************************************************/

        /// <summary>
        /// Create data of the method's coverage
        /// </summary>
        public CoverageData()
        {
            BlockByPart = new Dictionary<int, float>();
            PointToBlockEnds = new Dictionary<string, int>();
        }
    }
}
