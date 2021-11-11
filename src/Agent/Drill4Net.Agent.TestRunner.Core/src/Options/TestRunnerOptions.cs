using Drill4Net.Configuration;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Options for the TestRunner subsystem
    /// </summary>
    public class TestRunnerOptions : AbstractOptions
    {
        /// <summary>
        /// Target name (ID)
        /// </summary>
        public string Target { get; set; }

        /// <summary>
        /// URL for Admin side
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// File path for the Test assembly
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Options exclusive for debug
        /// </summary>
        public TestRunnerDebugOptions Debug { get; set; }
    }
}
