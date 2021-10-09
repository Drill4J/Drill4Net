using System;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// 'type' tests info
    /// </summary>
    [Serializable]
    public class TestSummary
    {
        /// <summary>
        /// 'type' tests coverage info
        /// </summary>
        public TestCoverage Coverage { get; set; }

        /// <summary>
        /// Number of 'type' tests in the build
        /// </summary>
        public int TestCount { get; set; }

        /// <summary>
        /// 'type' tests duration
        /// </summary>
        public long Duration { get; set; }
    }
}
