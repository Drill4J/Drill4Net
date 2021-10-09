using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// All key metrics of the build
    /// </summary>
    [Serializable]
    public class SummaryInfo
    {
        /// <summary>
        /// Total Build coverage percentage
        /// </summary>
        public double Coverage { get; set; }

        /// <summary>
        /// Probes info for about its coverage
        /// </summary>
        public long CoverageCount { get; set; }

        /// <summary>
        /// Methods info
        /// </summary>
        public long MethodCount { get; set; }

        /// <summary>
        /// Comparison with parent build. Arrow has 3 states: UNCHANGED, INCREASED, DECREASED
        /// </summary>
        public string Arrow { get; set; }

        /// <summary>
        /// Risks methods info
        /// </summary>
        public RiskCounter RiskCounts { get; set; }

        /// <summary>
        /// Test duration time
        /// </summary>
        public long TestDuration { get; set; }

        /// <summary>
        /// Tests info
        /// </summary>
        public List<TestInfo> Tests { get; set; }

        /// <summary>
        /// Recommended tests to run info
        /// </summary>
        public TestToRunInfo TestsToRun { get; set; }

        /// <summary>
        /// Text: -Run recommended tests to cover modified methods(if there are tests to run)
        /// -Update your tests to cover risks(if there are new methods)
        /// </summary>
        public List<string> Recommendations { get; set; }

        /*******************************************************************/

        public override string ToString()
        {
            return $"{Arrow} -> methods: {MethodCount}; tests: {Tests.Count}; risks: {RiskCounts}";
        }
    }
}
