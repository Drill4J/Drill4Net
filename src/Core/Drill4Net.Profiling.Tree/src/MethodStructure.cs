using System;
using System.Collections.Generic;

namespace Drill4Net.Profiling.Tree
{
    /// <summary>
    /// Structure of method for the further coverage calculations
    /// </summary>
    [Serializable]
    public class MethodStructure
    {
        /// <summary>
        /// Binding the cross-point by Uid to last index of the block's range in the probe list of method
        /// </summary>
        public Dictionary<string, int> PointToBlockEnds { get; set; }

        /**************************************************************/

        /// <summary>
        /// Create data of the method's coverage
        /// </summary>
        public MethodStructure()
        {
            PointToBlockEnds = new Dictionary<string, int>();
        }
    }
}
