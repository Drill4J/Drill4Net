using System;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// 'type' tests coverage info
    /// </summary>
    [Serializable]
    public record TestInfo
    {
        /// <summary>
        /// User can specify test type before starting a test session. 
        /// By default, there are 2 types: AUTO, MANUAL
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 'type' tests info
        /// </summary>
        public TestSummary Summary { get; set; }
    }
}
