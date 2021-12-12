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
        /// DEFAULT assembly to run tests if no builds still exists in Drill service
        /// </summary>
        public string DefaultAssemblyName { get; set; }

        /// <summary>
        /// The parallel execution is restricted by default (if no builds still exists in Drill service)?
        /// </summary>
        public bool DefaultParallelRestrict { get; set; }

        /// <summary>
        /// Options exclusively for debug
        /// </summary>
        public TestRunnerDebugOptions Debug { get; set; }
    }
}
