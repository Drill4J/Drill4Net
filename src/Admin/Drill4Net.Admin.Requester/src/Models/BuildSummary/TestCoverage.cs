using System;

namespace Drill4Net.Admin.Requester
{
    /// <summary>
    /// 'type' tests coverage info
    /// </summary>
    [Serializable]
    public record TestCoverage
    {
        /// <summary>
        /// Tests coverage percentage
        /// </summary>
        public double Percentage { get; set; }

        /// <summary>
        /// Methods info of 'type' tests
        /// </summary>
        public MethodCounter MethodCount { get; set; }

        /// <summary>
        /// Probes info of 'type' tests
        /// </summary>
        public ProbeCounter Count { get; set; }

        /***********************************************************************/

        public override string ToString()
        {
            return $"{MethodCount} methods -> {Percentage}% by {Count} probes";
        }
    }
}
