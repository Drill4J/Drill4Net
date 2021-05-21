using System.Collections.Generic;

namespace Drill4Net.Common
{
    /// <summary>
    /// Options for Test Engine
    /// </summary>
    public class VersionOptions
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

        public VersionOptions()
        {
            Targets = new Dictionary<string, MonikerData>();
        }
    }
}
