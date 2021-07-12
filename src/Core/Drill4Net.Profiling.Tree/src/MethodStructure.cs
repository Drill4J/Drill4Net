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
        /// Binding the cross-point by Uid to the end of the IL code block
        /// in the probe list (as business index)
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
