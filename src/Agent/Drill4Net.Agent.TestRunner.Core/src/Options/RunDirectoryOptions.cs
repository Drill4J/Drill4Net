using System;
using System.Collections.Generic;

namespace Drill4Net.Agent.TestRunner.Core
{
    /// <summary>
    /// Options for run test on Directory level, which can contains several test assemblies
    /// </summary>
    [Serializable]
    public class RunDirectoryOptions
    {
        /// <summary>
        /// Directory for the injected tests (full path)
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Assemblies with the injected tests
        /// </summary>
        public List<RunAssemblyOptions> Assemblies { get; set; }

        /// <summary>
        /// The parallel execution is restricted by default (if no builds still exists in Drill service)
        /// on current directory level
        /// </summary>
        public bool DefaultParallelRestrict { get; set; }
    }
}
