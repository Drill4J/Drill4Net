using System;
using System.Collections.Generic;

namespace Drill4Net.Configuration
{
    /// <summary>
    /// Parameters for the target object versions to be processed
    /// </summary>
    [Serializable]
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

        /// <summary>
        /// Create parameters for the target object versions to be processed
        /// </summary>
        public VersionOptions()
        {
            Targets = new Dictionary<string, MonikerData>();
        }
    }
}
