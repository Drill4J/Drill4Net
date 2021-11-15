using Drill4Net.Configuration;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Options for the TestRunner subsystem
    /// </summary>
    public class TestRunnerOptions : AbstractOptions
    {
        /// <summary>
        /// Directory for the injected tests
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// Options exclusively for debug
        /// </summary>
        public TestRunnerDebugOptions Debug { get; set; }
    }
}
