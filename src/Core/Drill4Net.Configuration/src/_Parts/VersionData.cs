using System;
using System.Collections.Generic;

namespace Drill4Net.Configuration
{
    /// <summary>
    /// Parameters for the target object versions to be processed
    /// </summary>
    [Serializable]
    public class VersionData
    {
        /// <summary>
        /// Base directory of the testing targets (use only for tests' system)
        /// </summary>
        public string Directory { get; set; }

        /// <summary>
        /// What will be tested - by target moniker/assembly/class
        /// </summary>
        public Dictionary<string, MonikerData> Targets { get; set; }

        /******************************************************/

        /// <summary>
        /// Create parameters for the target object versions to be processed
        /// </summary>
        public VersionData()
        {
            Targets = new Dictionary<string, MonikerData>();
        }
    }
}
