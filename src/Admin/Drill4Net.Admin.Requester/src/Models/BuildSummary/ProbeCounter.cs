using System;

namespace Drill4Net.Admin.Requester
{
    /// <summary>
    /// Probes info of 'type' tests
    /// </summary>
    [Serializable]
    public record ProbeCounter
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
