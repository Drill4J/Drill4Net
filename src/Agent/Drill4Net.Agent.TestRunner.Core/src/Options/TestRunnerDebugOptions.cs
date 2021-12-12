using Drill4Net.Configuration;

namespace Drill4Net.Agent.TestRunner.Core
{
    public class TestRunnerDebugOptions : IDebugOptions
    {
        public bool Disabled { get; set; }

        /// <summary>
        /// Use fake data
        /// </summary>
        public bool IsFake { get; set; }
    }
}
