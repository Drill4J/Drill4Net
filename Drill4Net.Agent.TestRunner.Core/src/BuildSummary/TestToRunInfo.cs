using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Recommended tests to run info
    /// </summary>
    [Serializable]
    public record TestToRunInfo
    {
        /// <summary>
        /// Number of tests to run
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Test types specified by User, e.g. "MANUAL": 2
        /// </summary>
        public Dictionary<string, int> ByType { get; set; }

        /*******************************************************************/

        public override string ToString()
        {
            return $"count = {Count}";
        }
    }
}