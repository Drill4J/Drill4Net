using System;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Probes info of 'type' tests
    /// </summary>
    [Serializable]
    public class ProbeCounter
    {
        /// <summary>
        /// Number of covered methods by 'type' test 
        /// </summary>
        public int Covered { get; set; }

        /// <summary>
        /// Total number of methods in the build
        /// </summary>
        public int Total { get; set; }
    }
}
