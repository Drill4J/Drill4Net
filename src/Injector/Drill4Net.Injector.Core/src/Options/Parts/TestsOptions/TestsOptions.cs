using System.Collections.Generic;

namespace Drill4Net.Injector.Core
{
    /// <summary>
    /// Options for Test Engine
    /// </summary>
    public class TestsOptions
    {
        /// <summary>
        /// Base directory of the testing targets
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// What will be tested - by target moniker/assembly/class
        /// </summary>
        public Dictionary<string, MonikerData> Targets { get; set; }

        /******************************************************/

        public TestsOptions()
        {
            Targets = new Dictionary<string, MonikerData>();
        }
    }
}
